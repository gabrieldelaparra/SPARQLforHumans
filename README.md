# SPARQL for Humans

Procesamiento de dump de ntriples.

Por el momento cumple dos tareas:
1. Filtra un dump de nt bajo ciertos criterios.
2. Convierte un dump en un `LuceneIndex` y los clasifica según PageRank.

Va con los tests.

## Criterios de filtrado
- Por cada entidad:
    - Label (@en only)
    - Description (@en only)
    - AltLabel (@en only)
    - InstanceOf (P31)
    - Todas las propiedades.

## Criterios de indexado
- Por cada Entity crea un documento. 
- Toma los items filtrados anteriormente con fields.
- Corre un algoritmo de Pagerank para darles el boost (por documento).

## Limitaciones
Por el momento, funciona como librería únicamente.\
No tiene la CLI para procesar directamente un dump.\
Se pueden correr los tests para entender que hace el código.

## Requisitos
- dotnet core 2.1

## Como correrlo
1. Clonar el repositorio
2. `dotnet build`
3. `dotnet test`

Al darle `dotnet build`, descargará las siguientes dependencias:
- [Lucene.Net](https://github.com/apache/lucenenet)
- [dotNetRDF](https://github.com/dotnetrdf/dotnetrdf)
- [NLog](https://github.com/NLog/NLog)
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)

Para pruebas con un dump completo, es necesario [descargar un dump](https://dumps.wikimedia.org/wikidatawiki/entities/).\
Puede trabajar con un archive `.nt.gz`.
