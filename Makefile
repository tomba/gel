
all:
	make -C Core
	make -C Compiler
	make -C Gel

clean:
	make -C Core clean
	make -C Compiler clean
	make -C Gel clean
