[![npm package](https://img.shields.io/npm/v/com.vel.velutils-ovr)](https://www.npmjs.com/package/com.vel.velutils-ovr)
[![openupm](https://img.shields.io/npm/v/com.vel.velutils-ovr?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.vel.velutils-ovr/)
![Tests](https://github.com/vel/velutils-ovr/workflows/Tests/badge.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

# VelUtils-OVR

Wrappers, adaptations, extension of Oculus/Meta specific packages as VelUtils might interact with them

- [How to use](#how-to-use)
- [Install](#install)
  - [via npm](#via-npm)
  - [via OpenUPM](#via-openupm)
  - [via Git URL](#via-git-url)
  - [Tests](#tests)
- [Configuration](#configuration)

<!-- toc -->

## How to use

Chuck SharedOrigin prefab into your scene and point it at your local player GameObject (rig). Does rely on Meta Shared Anchors which requires some project setup in order to use (https://developer.oculus.com/documentation/unity/unity-shared-spatial-anchors/)

## Install

### via npm

Open `Packages/manifest.json` with your favorite text editor. Add a [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) and following line to dependencies block:
```json
{
  "scopedRegistries": [
    {
      "name": "npmjs",
      "url": "https://registry.npmjs.org/",
      "scopes": [
        "com.vel"
      ]
    }
  ],
  "dependencies": {
    "com.vel.velutils-ovr": "1.0.0"
  }
}
```
Package should now appear in package manager.

### via OpenUPM

The package is also available on the [openupm registry](https://openupm.com/packages/com.vel.velutils-ovr). You can install it eg. via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.vel.velutils-ovr
```

### via Git URL

Open `Packages/manifest.json` with your favorite text editor. Add following line to the dependencies block:
```json
{
  "dependencies": {
    "com.vel.velutils-ovr": "https://github.com/vel/velutils-ovr.git"
  }
}
```

### Tests

The package can optionally be set as *testable*.
In practice this means that tests in the package will be visible in the [Unity Test Runner](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html).

Open `Packages/manifest.json` with your favorite text editor. Add following line **after** the dependencies block:
```json
{
  "dependencies": {
  },
  "testables": [ "com.vel.velutils-ovr" ]
}
```

## Configuration

*Work In Progress*

## License

MIT License

Copyright © 2023 VEL
