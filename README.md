[![codecov](https://codecov.io/gh/vroyibg/lwt/branch/master/graph/badge.svg)](https://codecov.io/gh/vroyibg/lwt)


This project is a simple clone of [Lwt](https://sourceforge.net/projects/lwt/)

Try demo here
https://lwt.hai.fyi

What it doesn't support:
* Word splitting/joining.
* Word review.
* Back up / restore.
* Custom language.
* Search, tags.

What it does better:
* Automated words splitting base on dictionary.
* Faster text loading speed.
* More detailed learning statuses of texts.
* Better statistics why for texts why reading.
* Unlimited text length (You can import a whole book into a text, so long text is not needed) without any affects on performance while reading.
* Intergrated dictionary api for supported languages.

# Getting started
* Clone the project
* Set the connection string env variable
```shell
export LWTDB="Server=localhost;Database=lwt;User=root;Password=;"
```
* Start the server by running
```shell
dotnet start
```
