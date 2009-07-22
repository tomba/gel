
SOURCES := $(shell grep "Compile Include" Gel.csproj | cut -d \" -f 2 | tr \\ / | tr ";" " ")

REFS = -r:Antlr3.Runtime.dll -r:Gel.Core.dll -r:Gel.Compiler.dll -r:Mono.GetOptions

all: bin/Debug/Gel.exe

bin/Debug/Gel.exe: $(SOURCES) ../Core/bin/Debug/Gel.Core.dll ../Compiler/bin/Debug/Gel.Compiler.dll
	mkdir -p bin/Debug
	cp -u ../Core/bin/Debug/* bin/Debug/
	cp -u ../Compiler/bin/Debug/* bin/Debug/
	cp -u ../Compiler/antlr-3.0b5/*.dll bin/Debug/
	gmcs -debug+ -t:exe -lib:bin/Debug -out:bin/Debug/Gel.exe $(SOURCES) $(REFS)

run:
	mono bin/Debug/Gel.exe

clean:
	rm -rf bin
