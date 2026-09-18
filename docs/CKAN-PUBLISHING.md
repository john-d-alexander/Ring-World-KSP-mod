# CKAN metadata for v1.1.3

Submit `distribution/NivenRingworld.netkan` to the CKAN maintainers handling the existing indexing request. It downloads the GitHub release, reads the bundled AVC version file (KSP 1.12.5), installs only GameData/NivenRingworld, and requires Harmony2 >= 2.2.1.0. Release archives contain no Harmony or Cyla files. Metadata preparation does not itself publish the mod on CKAN.

Cyla is an optional visual backend, not a required runtime dependency. Missing or unsupported Cyla falls back to Original. Once blackrack and the CKAN team approve Cyla's listing, add its **actual assigned identifier** under `suggests`; do not guess an identifier or invent a dependency that CKAN cannot resolve. Contacting the author/submitting metadata remains with the mod author.

Manual users must install Harmony separately. Cyla 1.1.0 is the supported optional version. Retain existing dependency installations when upgrading. Breaking Ground remains optional.

The GitHub release includes a separate version-specific .ckan asset for maintainer review/local installation; it is not embedded in the mod ZIP. The .netkan file supports automatic indexing of future releases. Official listing still requires maintainer acceptance. Windows x64 / D3D11 is the tested platform; cross-platform shader behavior is unverified.

Sources: https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md and https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/Harmony2/Harmony2-2.2.1.0.ckan .
