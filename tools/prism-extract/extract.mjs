// Loads PrismJS grammars and writes them to out/grammars.json as an object graph.
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { allIds, coreIds, createEnv, createEnvFor, hookSourceIds, mainIds, modifierIds, prismVersion, probeTemplating, titleOf } from './prism-env.mjs';

const languages = JSON.parse(await readFile(new URL('./languages.json', import.meta.url), 'utf8'));
const unknownIds = languages.map(l => l.id).filter(id => !allIds.includes(id) && !coreIds.includes(id));
if (unknownIds.length > 0) throw new Error(`Unknown language ids: ${unknownIds.join(', ')}`);

// The graph is shared by all environments. Grammars and patterns are objects here, so each environment keeps its own
// table from a PrismJS object to the index in the graph.
const grammars = [];
const patterns = [];

const toArray = value => Array.isArray(value) ? value : [value];

// Mirrors Prism.tokenize: "rest" entries overwrite in place or are appended, and "rest" itself is dropped.
const resolveEntries = grammar => {
  const entries = new Map(Object.keys(grammar).filter(key => key !== 'rest').map(key => [key, grammar[key]]));
  if (grammar.rest) {
    for (const key in grammar.rest) {
      if (key !== 'rest') entries.set(key, grammar.rest[key]);
    }
  }
  return entries;
};

// A string that is the same for two grammars if and only if they behave the same.
const canonicalForm = grammar => {
  const numbers = new Map();
  const visitGrammar = g => {
    if (numbers.has(g)) return `@${numbers.get(g)}`;
    numbers.set(g, numbers.size);
    const tokens = [];
    for (const [name, value] of resolveEntries(g)) {
      if (!value) continue;
      tokens.push(`${JSON.stringify(name)}:${toArray(value).map(visitPattern).join('|')}`);
    }
    return `{${tokens.join(',')}}`;
  };
  const visitPattern = p => {
    const regex = p instanceof RegExp ? p : p.pattern;
    const flags = p instanceof RegExp ? [] : [p.lookbehind, p.greedy, JSON.stringify(p.alias ?? null), p.inside ? visitGrammar(p.inside) : ''];
    return [regex.source, regex.flags, ...flags].join('~');
  };
  return visitGrammar(grammar);
};

const createContext = (resolveExternal = () => undefined) => ({ grammarIds: new Map(), patternIds: new Map(), resolveExternal });

const visitGrammar = (grammar, context) => {
  if (context.grammarIds.has(grammar)) return context.grammarIds.get(grammar);
  const external = context.resolveExternal(grammar);
  if (external !== undefined) {
    context.grammarIds.set(grammar, external);
    return external;
  }
  const id = grammars.length;
  context.grammarIds.set(grammar, id);
  const node = { tokens: [] };
  grammars.push(node);
  for (const [name, value] of resolveEntries(grammar)) {
    if (!value) continue;
    node.tokens.push({ name, patterns: toArray(value).map(p => visitPattern(p, context)) });
  }
  return id;
};

const visitPattern = (patternObj, context) => {
  if (context.patternIds.has(patternObj)) return context.patternIds.get(patternObj);
  const id = patterns.length;
  context.patternIds.set(patternObj, id);
  const regex = patternObj instanceof RegExp ? patternObj : patternObj.pattern;
  const node = { source: regex.source, flags: regex.flags };
  patterns.push(node);
  if (!(patternObj instanceof RegExp)) {
    if (patternObj.lookbehind) node.lookbehind = true;
    if (patternObj.greedy) node.greedy = true;
    if (patternObj.alias) node.alias = toArray(patternObj.alias);
    if (patternObj.inside) node.inside = visitGrammar(patternObj.inside, context);
  }
  return id;
};

const entryOf = (env, context, { id, className }) => {
  const entry = { id, className, title: titleOf(id), grammar: visitGrammar(env.Prism.languages[id], context) };
  const templating = probeTemplating(env, id);
  if (templating) {
    entry.templating = {
      source: templating.pattern.source,
      flags: templating.pattern.flags,
      hasFilter: templating.hasFilter,
      grammar: visitGrammar(templating.grammar, context),
    };
  }
  return entry;
};

// 1. The main environment has every language except the components that rewrite other languages.
const mainEnv = createEnv(mainIds);
const mainContext = createContext();
const entries = new Map();
for (const language of languages.filter(l => !modifierIds.includes(l.id))) {
  entries.set(language.id, entryOf(mainEnv, mainContext, language));
}
const mainCanonicalForms = new Map(
  [...entries.keys()].map(id => [id, canonicalForm(mainEnv.Prism.languages[id])]));

// 2. A language that rewrites another one, such as jsdoc, gets its own environment. The languages it refers to are
//    shared with the main environment if they are the same there, and copied if the rewriting changed them.
for (const language of languages.filter(l => modifierIds.includes(l.id))) {
  const env = createEnvFor(language.id);
  const languageById = new Map([...entries.keys()].map(id => [env.Prism.languages[id], id]));
  const context = createContext(grammar => {
    const id = languageById.get(grammar);
    return id !== undefined && canonicalForm(grammar) === mainCanonicalForms.get(id) ? entries.get(id).grammar : undefined;
  });
  entries.set(language.id, entryOf(env, context, language));
}

const roots = languages.map(l => entries.get(l.id));
await mkdir(new URL('./out/', import.meta.url), { recursive: true });
await writeFile(
  new URL('./out/grammars.json', import.meta.url),
  JSON.stringify({ prismVersion, hookSourceIds, languages: roots, grammars, patterns }, null, 2) + '\n');
console.log(`Extracted ${roots.length} languages, ${grammars.length} grammars, ${patterns.length} patterns (prismjs ${prismVersion}).`);
