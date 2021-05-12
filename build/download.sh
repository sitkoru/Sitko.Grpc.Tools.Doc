#!/bin/sh

export PROTOC_VERSION=3.16.0
export PROTOC_GEN_DOC_VERSION=1.4.1
export PROTOC_GEN_DOC_GO_VERSION=1.15.2

# download win64
curl -L https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOC_VERSION}/protoc-${PROTOC_VERSION}-win64.zip -o /tmp/protoc-${PROTOC_VERSION}-win64.zip
curl -L https://github.com/pseudomuto/protoc-gen-doc/releases/download/v$PROTOC_GEN_DOC_VERSION/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.windows-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -o /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.windows-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz
rm -rf ../src/Sitko.Grpc.Tools.Doc/Tools/windows_x64
mkdir -p ../src/Sitko.Grpc.Tools.Doc/Tools/windows_x64
unzip /tmp/protoc-${PROTOC_VERSION}-win64.zip -d /tmp/win64
mv /tmp/win64/bin/protoc.exe ../src/Sitko.Grpc.Tools.Doc/Tools/windows_x64/protoc.exe
cp -a /tmp/win64/include/google ../src/Sitko.Grpc.Tools.Doc/Proto
tar -xzvf /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.windows-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -C /tmp
mv /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.windows-amd64.go$PROTOC_GEN_DOC_GO_VERSION/protoc-gen-doc.exe ../src/Sitko.Grpc.Tools.Doc/Tools/windows_x64/protoc-gen-doc.exe
rm -rf /tmp/*

# download linux64
curl -L https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOC_VERSION}/protoc-${PROTOC_VERSION}-linux-x86_64.zip -o /tmp/protoc-${PROTOC_VERSION}-linux-x86_64.zip
curl -L https://github.com/pseudomuto/protoc-gen-doc/releases/download/v$PROTOC_GEN_DOC_VERSION/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.linux-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -o /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.linux-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz
rm -rf ../src/Sitko.Grpc.Tools.Doc/Tools/linux_x64
mkdir -p ../src/Sitko.Grpc.Tools.Doc/Tools/linux_x64
unzip /tmp/protoc-${PROTOC_VERSION}-linux-x86_64.zip -d /tmp/linux-x86_64
mv /tmp/linux-x86_64/bin/protoc ../src/Sitko.Grpc.Tools.Doc/Tools/linux_x64/protoc
tar -xzvf /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.linux-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -C /tmp
mv /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.linux-amd64.go$PROTOC_GEN_DOC_GO_VERSION/protoc-gen-doc ../src/Sitko.Grpc.Tools.Doc/Tools/linux_x64/protoc-gen-doc
rm -rf /tmp/*

# download osx64
curl -L https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOC_VERSION}/protoc-${PROTOC_VERSION}-osx-x86_64.zip -o /tmp/protoc-${PROTOC_VERSION}-osx-x86_64.zip
curl -L https://github.com/pseudomuto/protoc-gen-doc/releases/download/v$PROTOC_GEN_DOC_VERSION/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.darwin-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -o /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.darwin-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz
rm -rf ../src/Sitko.Grpc.Tools.Doc/Tools/macosx_x64
mkdir -p ../src/Sitko.Grpc.Tools.Doc/Tools/macosx_x64
unzip /tmp/protoc-${PROTOC_VERSION}-osx-x86_64.zip -d /tmp/osx-x86_64
mv /tmp/osx-x86_64/bin/protoc ../src/Sitko.Grpc.Tools.Doc/Tools/macosx_x64/protoc
tar -xzvf /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.darwin-amd64.go$PROTOC_GEN_DOC_GO_VERSION.tar.gz -C /tmp
mv /tmp/protoc-gen-doc-$PROTOC_GEN_DOC_VERSION.darwin-amd64.go$PROTOC_GEN_DOC_GO_VERSION/protoc-gen-doc ../src/Sitko.Grpc.Tools.Doc/Tools/macosx_x64/protoc-gen-doc
rm -rf /tmp/*

#https://github.com/protocolbuffers/protobuf/releases/download/v3.14.0/protoc-3.14.0-linux-x86_64.zip
#https://github.com/protocolbuffers/protobuf/releases/download/v3.14.0/protoc-3.14.0-linux-x86_32.zip
#https://github.com/protocolbuffers/protobuf/releases/download/v3.14.0/protoc-3.14.0-osx-x86_64.zip

#https://github.com/pseudomuto/protoc-gen-doc/releases/download/v1.3.2/protoc-gen-doc-1.3.2.windows-amd64.go1.12.6.tar.gz
#https://github.com/pseudomuto/protoc-gen-doc/releases/download/v1.3.2/protoc-gen-doc-1.3.2.linux-amd64.go1.12.6.tar.gz
#https://github.com/pseudomuto/protoc-gen-doc/releases/download/v1.3.2/protoc-gen-doc-1.3.2.darwin-amd64.go1.12.6.tar.gz
