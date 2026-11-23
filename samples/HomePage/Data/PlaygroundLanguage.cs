using Toolbelt.Blazor.Prismazor;

namespace HomePage.Data;

/// <summary>
/// A language that the playground offers, with a short example of code.
/// </summary>
/// <param name="DisplayName">The name in the drop-down list.</param>
/// <param name="PropertyName">The name of the property of <see cref="PrismazorLanguages"/>, which is shown in the usage example.</param>
/// <param name="Grammar">The grammar of the language.</param>
/// <param name="Code">The example code.</param>
public sealed record PlaygroundLanguage(string DisplayName, string PropertyName, Grammar Grammar, string Code)
{
    public static PlaygroundLanguage Default => All[0];

    public static IReadOnlyList<PlaygroundLanguage> All { get; } =
    [
        new("C#", nameof(PrismazorLanguages.CSharp), PrismazorLanguages.CSharp, """
            using System.Linq;

            namespace Demo;

            public record Person(string Name, int Age);

            public static class Program
            {
                public static void Main()
                {
                    var people = new[] { new Person("Ann", 31), new Person("Bob", 27) };
                    foreach (var p in people.OrderBy(p => p.Age))
                    {
                        Console.WriteLine($"{p.Name} is {p.Age} years old.");
                    }
                }
            }
            """),

        new("Razor", nameof(PrismazorLanguages.Cshtml), PrismazorLanguages.Cshtml, """
            @page "/counter"

            <h1>Counter</h1>
            <p role="status">Current count: @currentCount</p>
            <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

            @code {
                private int currentCount = 0;

                private void IncrementCount() => currentCount++;
            }
            """),

        new("F#", nameof(PrismazorLanguages.Fsharp), PrismazorLanguages.Fsharp, """
            open System

            type Shape =
                | Circle of radius: float
                | Rectangle of width: float * height: float

            let area shape =
                match shape with
                | Circle r -> Math.PI * r * r
                | Rectangle (w, h) -> w * h

            [ Circle 1.0; Rectangle (2.0, 3.0) ]
            |> List.iter (fun s -> printfn "%f" (area s))
            """),

        new("Visual Basic", nameof(PrismazorLanguages.VisualBasic), PrismazorLanguages.VisualBasic, """
            Imports System

            Module Program
                Sub Main()
                    Dim names As String() = {"Ann", "Bob"}
                    For Each name In names
                        Console.WriteLine("Hello, " & name & "!")
                    Next
                End Sub
            End Module
            """),

        new("HTML", nameof(PrismazorLanguages.Markup), PrismazorLanguages.Markup, """
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <title>Hello</title>
            </head>
            <body>
              <!-- A comment -->
              <h1 class="title">Hello, &amp; welcome!</h1>
              <script>console.log("hi");</script>
            </body>
            </html>
            """),

        new("CSS", nameof(PrismazorLanguages.Css), PrismazorLanguages.Css, """
            :root { --accent: #512bd4; }

            .card {
              display: grid;
              gap: 1rem;
              color: var(--accent);
              border: 1px solid rgb(0 0 0 / 12%);
            }

            @media (prefers-color-scheme: dark) {
              .card:hover { background: #1f1f1f; }
            }
            """),

        new("SCSS", nameof(PrismazorLanguages.Scss), PrismazorLanguages.Scss, """
            $accent: #512bd4;

            @mixin card($radius: 4px) {
              border-radius: $radius;
            }

            .card {
              @include card(8px);
              color: darken($accent, 10%);

              &:hover { color: $accent; }
            }
            """),

        new("JavaScript", nameof(PrismazorLanguages.JavaScript), PrismazorLanguages.JavaScript, """
            const sum = (a, b) => a + b;

            async function load(url) {
              const res = await fetch(url);
              if (!res.ok) throw new Error(`Failed: ${res.status}`);
              return res.json();
            }

            console.log(sum(1, 2), /^\d+$/.test("42"));
            """),

        new("TypeScript", nameof(PrismazorLanguages.Typescript), PrismazorLanguages.Typescript, """
            interface User {
              id: number;
              name: string;
            }

            function greet<T extends User>(user: T): string {
              return `Hello, ${user.name}!`;
            }

            const users: User[] = [{ id: 1, name: "Ann" }];
            users.forEach(u => console.log(greet(u)));
            """),

        new("JSX", nameof(PrismazorLanguages.Jsx), PrismazorLanguages.Jsx, """
            function App({ items }) {
              return (
                <ul className="list">
                  {items.map(item => <li key={item.id}>{item.name}</li>)}
                </ul>
              );
            }
            """),

        new("JSON", nameof(PrismazorLanguages.Json), PrismazorLanguages.Json, """
            {
              "name": "Prismazor",
              "version": "1.0.0",
              "languages": 288,
              "themes": ["prism", "prism-okaidia"],
              "private": false
            }
            """),

        new("YAML", nameof(PrismazorLanguages.Yaml), PrismazorLanguages.Yaml, """
            name: unit tests
            on:
              push:
                branches: [main]
            jobs:
              test:
                runs-on: ubuntu-latest
                steps:
                  - uses: actions/checkout@v4
                  - run: dotnet test
            """),

        new("TOML", nameof(PrismazorLanguages.Toml), PrismazorLanguages.Toml, """
            [package]
            name = "prismazor"
            version = "1.0.0"
            authors = ["Ann <ann@example.com>"]

            [dependencies]
            serde = { version = "1.0", features = ["derive"] }
            """),

        new("Markdown", nameof(PrismazorLanguages.Markdown), PrismazorLanguages.Markdown, """"
            # Prismazor

            Highlight code with **no JavaScript**.

            - It works with Blazor Server and WebAssembly.
            - See the [project page](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor).

            ```csharp
            Console.WriteLine("Hello, Prismazor!");
            ```
            """"),

        new("SQL", nameof(PrismazorLanguages.Sql), PrismazorLanguages.Sql, """
            SELECT u.name, COUNT(o.id) AS orders
            FROM users u
            LEFT JOIN orders o ON o.user_id = u.id
            WHERE u.created_at >= '2026-01-01'
            GROUP BY u.name
            ORDER BY orders DESC;
            """),

        new("GraphQL", nameof(PrismazorLanguages.Graphql), PrismazorLanguages.Graphql, """
            query GetUser($id: ID!) {
              user(id: $id) {
                name
                posts(first: 3) { title }
              }
            }

            mutation AddUser($name: String!) {
              addUser(name: $name) { id }
            }
            """),

        new("Python", nameof(PrismazorLanguages.Python), PrismazorLanguages.Python, """
            from dataclasses import dataclass

            @dataclass
            class Point:
                x: float
                y: float

            def distance(a: Point, b: Point) -> float:
                return ((a.x - b.x) ** 2 + (a.y - b.y) ** 2) ** 0.5

            print(distance(Point(0, 0), Point(3, 4)))  # 5.0
            """),

        new("Java", nameof(PrismazorLanguages.Java), PrismazorLanguages.Java, """
            import java.util.List;

            public class Hello {
                public static void main(String[] args) {
                    List<String> names = List.of("Ann", "Bob");
                    for (String name : names) {
                        System.out.println("Hello, " + name + "!");
                    }
                }
            }
            """),

        new("Kotlin", nameof(PrismazorLanguages.Kotlin), PrismazorLanguages.Kotlin, """
            data class User(val name: String, val age: Int)

            fun main() {
                val users = listOf(User("Ann", 31), User("Bob", 27))
                users.sortedBy { it.age }
                    .forEach { println("${it.name} is ${it.age}") }
            }
            """),

        new("Swift", nameof(PrismazorLanguages.Swift), PrismazorLanguages.Swift, """
            struct User {
                let name: String
                var age: Int
            }

            let users = [User(name: "Ann", age: 31), User(name: "Bob", age: 27)]
            for user in users.sorted(by: { $0.age < $1.age }) {
                print("\(user.name) is \(user.age)")
            }
            """),

        new("Go", nameof(PrismazorLanguages.Go), PrismazorLanguages.Go, """
            package main

            import "fmt"

            func main() {
                ch := make(chan int)
                go func() { ch <- 42 }()
                fmt.Println("received", <-ch)
            }
            """),

        new("Rust", nameof(PrismazorLanguages.Rust), PrismazorLanguages.Rust, """
            use std::collections::HashMap;

            fn main() {
                let mut scores: HashMap<&str, u32> = HashMap::new();
                scores.insert("Ann", 31);
                for (name, score) in &scores {
                    println!("{name}: {score}");
                }
            }
            """),

        new("C", nameof(PrismazorLanguages.C), PrismazorLanguages.C, """
            #include <stdio.h>

            int main(void) {
                int total = 0;
                for (int i = 1; i <= 10; i++) {
                    total += i;
                }
                printf("total = %d\n", total);
                return 0;
            }
            """),

        new("C++", nameof(PrismazorLanguages.Cpp), PrismazorLanguages.Cpp, """
            #include <iostream>
            #include <vector>

            int main() {
                std::vector<int> values{3, 1, 2};
                for (const auto& v : values) {
                    std::cout << v << '\n';
                }
                return 0;
            }
            """),

        new("PHP", nameof(PrismazorLanguages.Php), PrismazorLanguages.Php, """
            <?php
            declare(strict_types=1);

            function greet(string $name): string {
                return "Hello, {$name}!";
            }

            echo greet('Ann'), PHP_EOL;
            """),

        new("Ruby", nameof(PrismazorLanguages.Ruby), PrismazorLanguages.Ruby, """
            class Greeter
              def initialize(name)
                @name = name
              end

              def greet
                puts "Hello, #{@name}!"
              end
            end

            Greeter.new("Ann").greet
            """),

        new("Dart", nameof(PrismazorLanguages.Dart), PrismazorLanguages.Dart, """
            class Counter {
              int _count = 0;

              void increment() => _count++;
              int get value => _count;
            }

            void main() {
              final c = Counter()..increment();
              print('count = ${c.value}');
            }
            """),

        new("Lua", nameof(PrismazorLanguages.Lua), PrismazorLanguages.Lua, """
            local function fib(n)
              if n < 2 then return n end
              return fib(n - 1) + fib(n - 2)
            end

            for i = 0, 10 do
              print(i, fib(i))
            end
            """),

        new("Bash", nameof(PrismazorLanguages.Bash), PrismazorLanguages.Bash, """
            #!/bin/bash
            set -e

            for file in ./src/*.cs; do
              echo "Checking $file"
              grep -q "TODO" "$file" && echo "  has a TODO"
            done
            """),

        new("PowerShell", nameof(PrismazorLanguages.Powershell), PrismazorLanguages.Powershell, """
            param([string]$Path = ".")

            Get-ChildItem -Path $Path -Filter *.cs -Recurse |
                Where-Object { $_.Length -gt 1KB } |
                ForEach-Object { Write-Host "$($_.Name) is large" }
            """),

        new("Dockerfile", nameof(PrismazorLanguages.Docker), PrismazorLanguages.Docker, """
            FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
            WORKDIR /src
            COPY . .
            RUN dotnet publish -c Release -o /app

            FROM mcr.microsoft.com/dotnet/aspnet:11.0
            COPY --from=build /app /app
            ENTRYPOINT ["dotnet", "/app/App.dll"]
            """),

        new("Diff", nameof(PrismazorLanguages.Diff), PrismazorLanguages.Diff, """
            --- a/Program.cs
            +++ b/Program.cs
            @@ -1,5 +1,5 @@
             var name = "world";
            -Console.WriteLine("Hello, " + name);
            +Console.WriteLine($"Hello, {name}!");
             return 0;
            """),
    ];
}
