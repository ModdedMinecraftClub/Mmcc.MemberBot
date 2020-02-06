#!/bin/bash

set -e

cd ModdedMinecraftClub.MemberBot
dotnet publish -c Release
cd ..

if [ -d Bot ]
then 
    cd Bot
    rm -r *
    cd ..
else
    mkdir Bot
fi

cp -r ModdedMinecraftClub.MemberBot/ModdedMinecraftClub.MemberBot.Bot/bin/Release/netcoreapp3.1/publish/* Bot/

printf "\n\nDone.\n"