
SOURCES := $(shell grep "Compile Include" Core.csproj | cut -d \" -f 2 | tr \\ /)

all: bin/Debug/Gel.Core.dll

bin/Debug/Gel.Core.dll: $(SOURCES)
	mkdir -p bin/Debug
	gmcs -debug+ -t:library -out:bin/Debug/Gel.Core.dll $(SOURCES)

clean:
	rm -rf bin
