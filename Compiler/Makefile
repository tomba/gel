
SOURCES := $(shell grep "Compile Include" Compiler.csproj | cut -d \" -f 2 | tr \\ / | tr ";" " ")

REFS = -r:antlr-3.0b5/Antlr3.Runtime.dll -r:../Core/bin/Debug/Gel.Core.dll

all: bin/Debug/Gel.Compiler.dll

bin/Debug/Gel.Compiler.dll: $(SOURCES)
	mkdir -p bin/Debug
	gmcs -debug+ -t:library -out:bin/Debug/Gel.Compiler.dll $(SOURCES) $(REFS)

clean:
	rm -rf bin
