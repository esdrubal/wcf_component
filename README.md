# wcf_component

build.sh builds dotnet/wcf to be ran with mono runtime and class libraries.
build.sh also runs all the tests that dotnet/wcf runs in OSX.

After running build.sh System.ServiceModel.dll can be found in bin/System.ServiceModel.

The generated System.ServiceModel.dll was not tested yet on the Android on iOS.

## How to build

build.sh depends on changes that are on mono/master.

If you already have an updated mono master installed on your system you can do:
```
git clone --recursive git@github.com:esdrubal/wcf_component.git
cd wcf_component
export MONO_PREFIX="/path/to/mono/install"
./build.sh
```

If you don't have mono master installed you can do:
```
git clone --recursive git@github.com:esdrubal/wcf_component.git
cd wcf_component

git clone git@github.com:mono/mono.git
cd mono
export MONO_PREFIX="$(pwd)/install/"
./autogen.sh --prefix=$MONO_PREFIX
make -j8
make install

cd ..
./build.sh
```
