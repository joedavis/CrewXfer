all: target-debug

release: CrewXfer.zip

CrewXfer.zip: target-release
	mkdir -p GameData/CrewXfer/Plugins
	cp References/CLSInterfaces.dll ./bin/Release/CrewXfer.dll ./GameData/CrewXfer/Plugins/
	cp ./CrewXferConfig.cfg ./GameData/CrewXfer/
	cp ./LICENSE ./GameData
	zip -r CrewXfer.zip GameData

target-release:
	xbuild /property:Configuration=Release

target-debug:
	xbuild

clean:
	rm -rf bin obj GameData CrewXfer.zip

.PHONY: all clean target-debug target-release
