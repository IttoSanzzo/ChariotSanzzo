#!/bin/bash

if [ -f 'Lavalink.jar' ]; then
	echo "Lavalink.jar exists."
else
	wget "https://github.com/lavalink-devs/Lavalink/releases/download/3.7.12/Lavalink.jar"
fi

java -jar Lavalink.jar