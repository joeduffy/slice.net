mkdir bin
ilasm /dll src\PtrUtils.il /out:bin\System.Slice.Core.netmodule
csc /t:module /debug:pdbonly /unsafe /o+ /addmodule:bin\System.Slice.Core.netmodule /out:bin\System.Slice.netmodule src\*.cs 
link /dll /out:bin\System.Slice.dll bin\System.Slice.Core.netmodule bin\System.Slice.netmodule 
csc /debug:pdbonly /unsafe /r:bin\System.Slice.dll /out:bin\System.Slice.Test.exe tests\*.cs

