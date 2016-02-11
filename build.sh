#!/bin/sh

BCL_DIR=../mono/mono8/mcs/class/lib/net_4_x
PCL_DIR=$BCL_DIR/Facades

BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SRC_DIR="$BASE_DIR/src"

XUNIT_ARGS="-lib:lib/xunit -r:xunit.core -r:xunit.assert -r:xunit.abstractions -r:xunit.execution.dotnet"

TEST_ARGS="-r:System.ServiceModel -r:UnitTests.Common -r:System.Runtime -r:System.Threading.Tasks -r:System.Runtime.Serialization $XUNIT_ARGS"
SCENARIO_ARGS="-r:System.ServiceModel -r:Infrastructure.Common -r:ScenarioTests.Common.dll -r:System.Runtime -r:System.Threading.Tasks -r:System.Runtime.Serialization $XUNIT_ARGS"

IGNORE_REFS="xunit System.ServiceModel.Primitives System.ServiceModel.Http System.ServiceModel.Duplex System.ServiceModel.Security System.ServiceModel.NetTcp"

build () {
   NAME=$1
   PROJECT_DIR=$2
   EXTRA=$3

   if [ -z "$PROJECT_DIR" ]; then
       CS_FILES=""
       REFS=""
   else
       CS_FILES="$(find $PROJECT_DIR -name '*.cs' | tr '\n' ' ')"
       REFS="$(python getrefs.py $PROJECT_DIR/project.json $IGNORE_REFS)"
   fi

   echo
   echo "Building $NAME"
   mcs -t:library /unsafe $EXTRA -lib:$OUT_DIR -lib:$PCL_DIR -lib:$BCL_DIR -out:$OUT_DIR/$NAME $REFS $CS_FILES
}

OUT_DIR="$BASE_DIR/bin/System.ServiceModel"
mkdir -p $OUT_DIR

build "System.Reflection.DispatchProxy.dll" \
    "external/corefx/src/System.Reflection.DispatchProxy/src" \
    "$SRC_DIR/System.Reflection.DispatchProxy/SR.cs"

DIR=external/corefx/src/System.Runtime.InteropServices.RuntimeInformation/src
build "System.Runtime.InteropServices.RuntimeInformation.dll" \
    "" \
    "$DIR/RuntimeInformation.OSX.cs $DIR/RuntimeInformation.cs $DIR/Architecture.cs $DIR/OSPlatform.cs src/System.Runtime.InteropServices.RuntimeInformation/SR.cs"

build "System.Private.ServiceModel.dll" \
    "external/wcf/src/System.Private.ServiceModel/src" \
    "-r:System.Runtime.Serialization.dll
    src/System.ServiceModel/SR.cs external/wcf/src/Common/src/System/NotImplemented.cs
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketTaskExtensions.cs
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveMessageFromResult.cs
    external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveFromResult.cs
    -d:FEATURE_CORECLR"

mv $OUT_DIR/System.Private.ServiceModel.dll $OUT_DIR/System.ServiceModel.dll

echo
echo "Building tests"

TEST_DIR="$BASE_DIR/bin/Tests"
mkdir -p $TEST_DIR
cp "$OUT_DIR"/* "$TEST_DIR"

OUT_DIR="$TEST_DIR"

build "xunit.netcore.extensions.dll" \
    "external/buildtools/src/xunit.netcore.extensions/" \
    "-r:System.Runtime -r:System.Threading.Tasks -d:FEATURE_CORECLR $XUNIT_ARGS"

build "UnitTests.Common.dll" \
    "external/wcf/src/System.Private.ServiceModel/tests/Common/Unit/" \
    "-r:System.ServiceModel -r:System.Runtime.Serialization"

build "System.Private.ServiceModel.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Unit/" "$TEST_ARGS"
build "System.ServiceModel.Primitives.Tests.dll" "external/wcf/src/System.ServiceModel.Primitives/tests/" "$TEST_ARGS"

build "Infrastructure.Common.dll" "external/wcf/src/System.Private.ServiceModel/tests/Common/Infrastructure/src/" "src/Infrastructure.Common/TestProperties.cs $TEST_ARGS"
build "Infrastructure.Common.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Common/Infrastructure/tests/" "-r:Infrastructure.Common $TEST_ARGS"

build "ScenarioTests.Common.dll" "external/wcf/src/System.Private.ServiceModel/tests/Common/Scenarios/" "-r:Infrastructure.Common $TEST_ARGS"

build "Binding.Custom.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Binding/Custom/" "$SCENARIO_ARGS"
build "Binding.Http.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Binding/Http/" "$SCENARIO_ARGS"
build "Client.ChannelLayer.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Client/ChannelLayer/" "$SCENARIO_ARGS"
build "Client.ClientBase.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Client/ClientBase/" "$SCENARIO_ARGS"
build "Client.ExpectedExceptions.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Client/ExpectedExceptions/" "-r:System.Net.Http $SCENARIO_ARGS"
build "Client.TypedClient.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Client/TypedClient/" "$SCENARIO_ARGS"
build "Contract.Data.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Contract/Data/" "$SCENARIO_ARGS"
build "Contract.Fault.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Contract/Fault/" "$SCENARIO_ARGS"
build "Contract.Message.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Contract/Message/" "$SCENARIO_ARGS"
build "Contract.Service.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Contract/Service/" "$SCENARIO_ARGS"
build "Contract.XmlSerializer.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Contract/XmlSerializer/" "$SCENARIO_ARGS"
build "Encoding.Encoders.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Encoding/Encoders/" "$SCENARIO_ARGS"
build "Encoding.MessageVersion.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Encoding/MessageVersion/" "$SCENARIO_ARGS"
build "Extensibility.WebSockets.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Extensibility/WebSockets/" "$SCENARIO_ARGS"
build "Security.TransportSecurity.Tests.dll" "external/wcf/src/System.Private.ServiceModel/tests/Scenarios/Security/TransportSecurity/" "$SCENARIO_ARGS"

build "System.ServiceModel.Duplex.Tests.dll" "external/wcf/src/System.ServiceModel.Duplex/tests/" "$TEST_ARGS"
build "System.ServiceModel.NetTcp.Tests.dll" "external/wcf/src/System.ServiceModel.NetTcp/tests/" "$TEST_ARGS"
build "System.ServiceModel.Http.Tests.dll" "external/wcf/src/System.ServiceModel.Http/tests/" "$TEST_ARGS"
build "System.ServiceModel.Security.Tests.dll" "external/wcf/src/System.ServiceModel.Security/tests/" "$TEST_ARGS"

echo
echo "Checking if all projects are compiled"

DLL_NOT_COMPILED=0
DLL_COUNT=0
for f in $(find external/wcf/src -name '*.csproj' | tr '\n' ' '); do
    DLL_NAME="$(echo $(cat $f | grep "<AssemblyName>" | sed 's/.*>\(.*\)<.*/\1/g'))"
    if test "${IGNORE_REFS#*$DLL_NAME}" == "$IGNORE_REFS" && [ ! -z "$DLL_NAME" ] && [ ! -e "$OUT_DIR/$DLL_NAME.dll" ]; then
        DLL_NOT_COMPILED=$(expr $DLL_NOT_COMPILED + 1)
        echo "  $f"
    fi
    DLL_COUNT=$(expr $DLL_COUNT + 1)
done

if [ $DLL_NOT_COMPILED -gt 0 ]; then
    echo "$DLL_NOT_COMPILED out of $DLL_COUNT projects were not compiled"
fi

echo
echo "Running tests"

cp lib/xunit/* $TEST_DIR

for f in $(find $TEST_DIR -name "*.Tests.dll"); do
    MONO_PATH="$BCL_DIR:$PCL_DIR:lib/xunit:$OUT_DIR" mono --debug lib/xunit.console.exe $f -notrait category=failing -notrait category=OuterLoop
done
