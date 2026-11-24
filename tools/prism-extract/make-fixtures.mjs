// Highlights every Fixtures/<language>/*.txt file of the test project with PrismJS and writes *.expected.html next to it.
import { readdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createEnvFor } from './prism-env.mjs';

const fixturesDir = fileURLToPath(new URL('../../tests/Toolbelt.Blazor.Prismizor.Test/Fixtures/', import.meta.url));
let count = 0;
for (const language of await readdir(fixturesDir)) {
  // The environment is the same as the one the grammar is extracted from.
  const { Prism } = createEnvFor(language);
  const grammar = Prism.languages[language];
  if (!grammar) throw new Error(`Unknown language folder: ${language}`);
  for (const file of (await readdir(path.join(fixturesDir, language))).filter(f => f.endsWith('.txt'))) {
    const code = await readFile(path.join(fixturesDir, language, file), 'utf8');
    const html = Prism.highlight(code, grammar, language);
    await writeFile(path.join(fixturesDir, language, file.replace(/\.txt$/, '.expected.html')), html);
    count++;
  }
}
console.log(`Wrote ${count} expected HTML files.`);
