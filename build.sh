OUT_DIR=lib
PCL_DIR=../mono/mono8/mcs/class/lib/net_4_x/Facades
BCL_DIR=../mono/mono8/mcs/class/lib/net_4_x

mkdir -p $OUT_DIR

PROJECT_DIR=external/corefx/src/System.Reflection.DispatchProxy/src
SOURCES="$PROJECT_DIR/System/Reflection/DispatchProxy.cs $PROJECT_DIR/System/Reflection/DispatchProxyGenerator.cs src/System.Reflection.DispatchProxy/SR.cs"
mcs -t:library $SOURCES -lib:$PCL_DIR -out:$OUT_DIR/System.Reflection.DispatchProxy.dll

PROJECT_DIR=external/wcf/src/System.Private.ServiceModel/src
REFS="-r:System.Runtime.Serialization.dll $(python getrefs.py $PROJECT_DIR/project.json)"
SOURCES="$(find $PROJECT_DIR -name "*.cs" | tr '\n' ' ') src/System.ServiceModel/SR.cs external/wcf/src/Common/src/System/NotImplemented.cs \
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketTaskExtensions.cs
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveMessageFromResult.cs
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveFromResult.cs"
# Cannot output to System.ServiceModel.dll directly because some assemblies have internals visible to it.
mcs -t:library /unsafe  $SOURCES -d:FEATURE_CORECLR -lib:$OUT_DIR -lib:$PCL_DIR -lib:$BCL_DIR $REFS -out:$OUT_DIR/System.ServiceModel2.dll
