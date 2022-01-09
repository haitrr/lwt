# syntax=docker/dockerfile:1

FROM ubuntu:20.04

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" apt-get install mecab libmecab-dev python3 -y python3-pip build-essential cmake

WORKDIR /app

COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt


COPY . .

CMD [ "python3", "-m" , "flask", "run", "--host=0.0.0.0", "--port=8080"]