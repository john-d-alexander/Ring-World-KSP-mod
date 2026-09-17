# Pinned Cyla dependency

Upstream release 1.1.0.0, commit 92223648e0674212e488e6fb977b5cffc63be869.

Cyla/ contains the unmodified release plugin, shader bundle and part. Source/ contains the matching published plugin source, original project/solution, README and GPLv3 license. PROVENANCE.json records upstream URLs and binary hashes. Shader source is not available upstream and is not included here.

The Ringworld build installs Cyla/ as GameData/Cyla and includes Source/ under ThirdParty/Cyla in the ZIP. It does not rebuild Cyla or execute the upstream project's local post-build commands. NivenRingworld contains an original adapter to the shader interface; upstream source is not compiled into NivenRingworld.dll. Keep this source/license/provenance with packaged builds. No nested Git checkout is required.
