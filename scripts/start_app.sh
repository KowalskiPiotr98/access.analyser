#!/bin/sh
cd /home/ec2-user/accessanalyserapp

# use systemd to start and monitor dotnet application
systemctl enable kestrel-aspnetcoreapp.service
systemctl start kestrel-aspnetcoreapp.service

# start apache
systemctl restart apache2.service
