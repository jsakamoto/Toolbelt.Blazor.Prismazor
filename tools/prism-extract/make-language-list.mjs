// Writes languages.json: every PrismJS language that has a grammar and that the generator can convert.
import { writeFile } from 'node:fs/promises';
import { createEnv, allIds, mainIds, modifierIds } from './prism-env.mjs';

// Languages the generator cannot convert yet, with the reason.
const unsupported = {};

const classNames = { markup: 'Markup', css: 'Css', javascript: 'JavaScript', csharp: 'CSharp' };
const pascalCase = id => id.split('-').map(part => part[0].toUpperCase() + part.slice(1)).join('');

const mainEnv = createEnv(mainIds);
const seen = new Map();
const result = [];
const skipped = [];
for (const id of allIds) {
  const env = modifierIds.includes(id) ? createEnv([...mainIds, id]) : mainEnv;
  const grammar = env.Prism.languages[id];
  // css-extras and the like only change another language, so they have no grammar of their own.
  if (!grammar || Object.keys(grammar).length === 0) { skipped.push(`${id} (no grammar)`); continue; }
  // cilkc and cilkcpp are the same object as c and cpp.
  if (seen.has(grammar)) { skipped.push(`${id} (same as ${seen.get(grammar)})`); continue; }
  seen.set(grammar, id);
  if (id in unsupported) { skipped.push(`${id} (${unsupported[id]})`); continue; }
  let className = classNames[id] ?? pascalCase(id);
  if (/^\d/.test(className)) className = `L${className}`;
  result.push({ id, className });
}

const duplicated = result.map(l => l.className).filter((name, i, names) => names.indexOf(name) !== i);
if (duplicated.length > 0) throw new Error(`Duplicated class names: ${duplicated.join(', ')}`);

await writeFile(new URL('./languages.json', import.meta.url), JSON.stringify(result, null, 2) + '\n');
console.log(`Wrote ${result.length} languages. Skipped: ${skipped.join(', ')}`);
