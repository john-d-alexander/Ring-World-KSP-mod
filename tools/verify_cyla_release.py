"""Cyla is external since 1.1.3; verify the new package policy instead of requiring bundled binaries."""
import pathlib, runpy
runpy.run_path(str(pathlib.Path(__file__).with_name('verify_release.py')),run_name='__main__')
