// Creates isolated PrismJS environments, so that grammars and hooks of one environment never leak into another.
import { createRequire } from 'node:module';
import { readFileSync } from 'node:fs';
import path from 'node:path';
import Module from 'node:module';

const require = createRequire(import.meta.url);
const prismDir = path.dirname(require.resolve('prismjs/package.json'));
const components = require('prismjs/components.json').languages;

export const prismVersion = require('prismjs/package.json').version;

/** The ids of all components of PrismJS. */
export const allIds = Object.keys(components).filter(k => k !== 'meta');

// A component that rewrites the grammar of another language in place, such as jsdoc and js-extras, is not loaded
// into the main environment. Otherwise "javascript" would differ from what a user gets by selecting only it.
// css, javascript and actionscript only add inlined languages to markup, so they stay in the main environment.
export const modifierIds = allIds.filter(id => [].concat(components[id].modify ?? []).some(target => target !== 'markup'));

/** The ids of the components that are loaded into the main environment. */
export const mainIds = allIds.filter(id => !modifierIds.includes(id));

// The languages that prism-core.js defines by itself, which are not in components.json.
// "plain" is an empty grammar, and "plaintext", "text", and "txt" are other names of it.
const coreLanguages = { plain: 'Plain text' };

/** The ids of the languages that prism-core.js defines by itself. */
export const coreIds = Object.keys(coreLanguages);

/** The title of a language, such as "C#". */
export const titleOf = id => coreLanguages[id] ?? components[id].title;

/** The ids of the languages whose own component file registers a before-tokenize or after-tokenize hook. */
export const hookSourceIds = allIds.filter(id => {
  const file = path.join(prismDir, 'components', `prism-${id}.js`);
  return /hooks\.add\(\s*['"](?:before|after)-tokenize['"]/.test(readFileSync(file, 'utf8'));
});

// The loader of PrismJS reloads markup-templating whenever it loads something else, so the wrapper has to be put on
// every time the file runs. It hooks the module loader for that.
let currentRecord = null;
const compileJs = Module._extensions['.js'];
Module._extensions['.js'] = function (module, filename) {
  compileJs(module, filename);
  if (currentRecord && filename.endsWith(`${path.sep}prism-markup-templating.js`)) wrapMarkupTemplating(global.Prism, currentRecord);
};

function wrapMarkupTemplating(Prism, record) {
  const original = Prism.languages['markup-templating'];
  Prism.languages['markup-templating'] = Object.defineProperties({}, {
    buildPlaceholders: {
      value(env, language, pattern, replaceFilter) {
        if (env.language === language && record.current) {
          record.current.push({ pattern, hasFilter: typeof replaceFilter === 'function' });
        }
        return original.buildPlaceholders(env, language, pattern, replaceFilter);
      },
    },
    tokenizePlaceholders: { value: (env, language) => original.tokenizePlaceholders(env, language) },
  });
}

/**
 * Loads the given components into a new PrismJS environment. The default bundle (core, markup, css, clike and
 * javascript) is always there, like in prism.js. Dependencies ("require") are loaded too.
 *
 * `record.current` collects the calls of `buildPlaceholders` of markup-templating, which lets the extractor learn
 * the patterns of the template languages without parsing their hooks.
 */
export function createEnv(ids) {
  for (const key of Object.keys(require.cache)) {
    if (key.startsWith(prismDir)) delete require.cache[key];
  }
  delete global.Prism;

  const record = { current: null };
  currentRecord = record;
  const Prism = require('prismjs');
  const loadLanguages = require('prismjs/components/index.js');
  loadLanguages.silent = true;
  loadLanguages(ids);
  currentRecord = null;
  return { Prism, record };
}

let mainEnv = null;

/**
 * Returns the environment that holds the grammar of the given language, which is the one in which it is defined.
 * It makes the environment the current one, because some hooks of PrismJS use the global `Prism` when they run.
 */
export function createEnvFor(id) {
  if (modifierIds.includes(id)) return createEnv([...mainIds, id]);
  mainEnv ??= createEnv(mainIds);
  global.Prism = mainEnv.Prism;
  return mainEnv;
}

/**
 * Runs the before-tokenize hooks of PrismJS for the given language and returns what they asked markup-templating to do.
 * It returns null if the language does not use markup-templating.
 */
export function probeTemplating({ Prism, record }, id) {
  const grammar = Prism.languages[id];
  // "<?" lets the hook of PHP, which skips code without it, go on.
  const env = { code: '<?php ?>', grammar, language: id };
  const calls = record.current = [];
  try {
    for (const callback of Prism.hooks.all['before-tokenize'] ?? []) callback(env);
  } finally {
    record.current = null;
  }
  if (calls.length === 0) return null;
  if (calls.length > 1) throw new Error(`${id}: more than one buildPlaceholders call.`);
  return { pattern: calls[0].pattern, hasFilter: calls[0].hasFilter, grammar: env.grammar };
}
