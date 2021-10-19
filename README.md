# SPARQL for Humans

# Using this repository

You will first need a Wikidata dump.

## Wikidata Dump

- Wikidata dump latest truthy (`.nt.gz` is larger, but faster for building the index.) https://dumps.wikimedia.org/wikidatawiki/entities/latest-truthy.nt.gz [~50GB]

- For testing you can use an internal file: `SPARQLforHumans\Sample500.nt`

Then some tools for compiling the source code.

## Development tools

- `dotnet SDK` https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-5.0.402-windows-x64-installer
- `git for windows SDK` https://github.com/git-for-windows/build-extra/releases/latest
> `Git For Windows SDK` since need an update version of `gzip` to sort the large output files)

For the `RDFExplorer` client, we will also need `node` https://nodejs.org/en/download/
> If your planning on running the benchamarks only, then `node` is not required.

### Set up gzip
On the `Git SDK-64` console (`Git for Windows SDK Console`)
Install `gzip` via `pacman`

``` bash
$ pacman -S gzip
```

We'll now need to clone the repository and build the project.

## Clone and build

On the `Git SDK-64` console
``` bash
$ git clone https://github.com/gabrieldelaparra/SPARQLforHumans.git

> Cloning into 'SPARQLforHumans'...
> [...]

$ cd SPARQLforHumans

$ dotnet build .

> [...]
> Build succeeded.
>     0 Warning(s)
>     0 Error(s)
```

Now we will run some tests to check that everything works.

## Test
``` bash
$ cd SparqlForHumans.UnitTests/

$ dotnet test

> [...]
> Passed!  - Failed:     0, Passed:   214, Skipped:     0, Total:   214, Duration: 9 s - SparqlForHumans.UnitTests.dll (netcoreapp3.1)
```

If any of the tests do not pass, you can create an issue and I will get in touch with you :)
Now we will run the Command Line Interface to filter and index our Wikidata dump.

## Command line interface

``` bash
$ cd ../SparqlForHumans.CLI/

$ dotnet run -- --version
> SparqlForHumans.CLI 1.0.0
```

For the following sections a given `Sample500.nt` file is given on the root folder of the repository.
To build the complete index (production), `latest-truthy.nt.gz` should be used.
Please note that filtering and indexing the `latest-truthy.nt.gz` will take some 40~80 hours, depending on your system.

## Filter

Filters an input file:
- Keeps all subjects that start with `http://www.wikidata.org/entity/`
- Keeps all predicates that start with `http://www.wikidata.org/prop/direct/`
  - and object starts with `http://www.wikidata.org/entity/`
- or predicate is `label`, `description` or `alt-label`
  - and object is literal and ends with `@en`.

These can be changed on the `SparqlForHumans.RDF\FilterReorderSort\TriplesFilterReorderSort.IsValidLine()` method.

To filter run:
``` bash
$ dotnet run -- -i ../Sample500.nt -f
```

The command for `sorting` is given in the console after filtering.
It will add the `.filterAll.gz` sufix as filtered output and `.filterAll-Sorted.gz` for sorting.

## Sort

Sorting takes `Sample500.filterAll.gz` as input and outputs `Sample500.filterAll-Sorted.gz`.

> The sorting command process gives no notifications about the status. Please be patient.

``` bash
$ gzip -dc Sample500.filterAll.gz | LANG=C sort -S 200M --parallel=4 -T tmp/ --compress-program=gzip | gzip > Sample500.filterAll-Sorted.gz
```

## Entities Index

After filtering and sorting, we can now create our index. As a note, both "`-e -p`" can be given together for the sample file to generate both Entities and Properties Index. For a large file, it is better to do them in 2 steps.

Entities Index will be created by default at `%homepath%\SparqlForHumans\LuceneEntitiesIndex\`

``` bash
$ dotnet run -- -i Sample500.filterAll-Sorted.gz -e
```

If `-p` was not used above, then we need to create the Properties Index.

## Properties Index

``` bash
$ dotnet run -- -i Sample500.filterAll-Sorted.gz -p
```
Properties Index will be created by default at `%homepath%\SparqlForHumans\LucenePropertiesIndex\`

Now our index is ready.
- We can now run our backend via `SparqlForHumans.Server/` using the `RDFExplorer` client.
- Or recreate the results from the paper via `SparqlForHumans.Benchmark/`.

## Run Server
The backend will listen to request from a modified version of `RDFExplorer`. First we will need to get the server running:

``` bash
$ cd ../SparqlForHumans.Server/

$ dotnet run
```

With the server running we can now start the client.

## Run Client: RDFExplorer

We will now need another console for this.

```
$ cd `path-for-the-client`

$ git clone https://github.com/gabrieldelaparra/RDFExplorer.git

$ cd RDFExplorer

$ npm install

$ npm start
```

Now browse to `http://localhost:4200/`

## Compare against the Wikidata Endpoint

With the full index we can compare our results agains the `Wikidata Endpoint`.
- `67` Properties (`{Prop}`) have been selected to run `4` type of queries (For a total of `268`)
  - `?var1 {Prop} ?var2 ; ?var1 ?prop ?var3 ;`
  - `?var1 {Prop} ?var2 ; ?var3 ?prop ?var1 ;`
  - `?var1 {Prop} ?var2 ; ?var2 ?prop ?var3 ;`
  - `?var1 {Prop} ?var2 ; ?var3 ?prop ?var2 ;`
- `268` queries are run against our `Local Index` and the `Remote Endpoint`.
- Running the benchmarks takes 3 hours, due to the 50 seconds timeout if the query cannot be completed on the Wikidata Endpoint.
- The details of the runs will be stored at `benchmark.json`.
- The time results will be summarized at `results.txt`.
- The time results, for each query, will be exported to a `points.csv`. Each row is a query. The `Id` of the query can be found on the `benchmark.json` file as `HashCode`.
- A qualitative comparison (`precision`, `recall`, `f1`), for each query, will be exported to `metrics.csv`. Each row is a query.

``` bash
$ cd ../SparqlForHumans.Benchmark/

$ dotnet run
```