#!/bin/bash

if pgrep "dotnet";then
	echo "dotnet process found killing"
	pkill "dotnet";
fi
