#!/bin/sh
cd /home/ec2-user/accessanalyserapp
nohup dotnet access.analyser.dll &>/dev/null &
