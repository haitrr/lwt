#!/bin/bash
if [[ $1 == '' ]]
then
    echo 'Please input provide password to replace'
    exit 0
fi

echo 'replace password'
cd ./src/Lwt/Utilities/
sed -i -e "s/\"q\"/\"$1\"/g" ./DatabaseSeeder.cs

cd ..

echo 'stop service'
sudo systemctl stop lwt-api.service

echo 'build app'
dotnet build -c freevn -o publish

echo 'start service'
sudo systemctl start lwt-api.service

echo 'service status'
sudo systemctl status lwt-api.service
