#!/bin/bash
dotnet build -c Release --no-incremental
dotnet publish -c Release -o ./out --no-build
cp ./out/wwwroot/index.html ./out/wwwroot/404.html
cp ./out/wwwroot/* ../docs -r
touch ../docs/.nojekyll
