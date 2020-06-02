#!/bin/sh
cd /home/ec2-user/accessanalyserapp
rm access.analyser.deps.json #if not deleted app requires Microsoft.Data.SqlClient.dll which is present
nohup dotnet access.analyser.dll --urls="http://0.0.0.0:5000;https://0.0.0.0:5001" &>/dev/null &
