// Copies the PrismJS themes and license into the library so that apps can use them as static web assets.
import { copyFile, mkdir, readdir } from 'node:fs/promises';
import { createRequire } from 'node:module';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const prismDir = path.dirname(require.resolve('prismjs/package.json'));
const libraryDir = fileURLToPath(new URL('../../src/Toolbelt.Blazor.Prismazor/', import.meta.url));
const themesDir = path.join(libraryDir, 'wwwroot', 'themes');

await mkdir(themesDir, { recursive: true });
const themes = (await readdir(path.join(prismDir, 'themes'))).filter(f => f.endsWith('.min.css'));
for (const theme of themes) {
  await copyFile(path.join(prismDir, 'themes', theme), path.join(themesDir, theme));
}
await copyFile(path.join(prismDir, 'LICENSE'), path.join(libraryDir, 'THIRD-PARTY-NOTICES-PRISM.txt'));
console.log(`Copied ${themes.length} themes.`);
