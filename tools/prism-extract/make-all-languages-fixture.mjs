// Highlights Fixtures/AllLanguages/sample.txt with every language and writes the result to expected.json.
import { readFile, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { createEnvFor } from './prism-env.mjs';

const dir = fileURLToPath(new URL('../../tests/Toolbelt.Blazor.Prismizor.Test/AllLanguages/', import.meta.url));
const sample = await readFile(`${dir}sample.txt`, 'utf8');
const languages = JSON.parse(await readFile(new URL('./languages.json', import.meta.url), 'utf8'));

const expected = {};
for (const { id } of languages) {
  const { Prism } = createEnvFor(id);
  expected[id] = Prism.highlight(sample, Prism.languages[id], id);
}
await writeFile(`${dir}expected.json`, JSON.stringify(expected, null, 1) + '\n');
console.log(`Wrote the expected output of ${languages.length} languages.`);
