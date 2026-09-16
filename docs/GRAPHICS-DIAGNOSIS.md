# Screenshot artifact diagnosis (2026-09-15)

This is a ranked hypothesis, not a confirmed hardware diagnosis. The supplied map screenshot shows straight, stepped green lines meeting at geometric boundaries. The flight screenshot shows large flat-colored polygonal regions and a diagonal boundary. Those patterns suggest depth competition, shadow-map precision or geometry/shader errors before memory exhaustion. The Ringworld map's overlapping shells were a real possible contributor and have been corrected separately; the reported vanilla occurrence must be isolated on stock terrain.

Observed system: Qualcomm Adreno X1-85, driver 31.0.137.0; KSP uses Unity 2019.4.18f1 and Direct3D 11 feature level 11.1. Windows reported 33,099,544 KiB usable RAM and 13,687,224 KiB free during inspection. These are a snapshot, not measurements at the instant of the screenshots. Unity's 16,161 MB VRAM report on this integrated GPU must not be interpreted as a separate dedicated memory bank; WMI's AdapterRAM=0 is likewise not proof of missing video memory.

The leading possibilities are:

1. Overlapping surfaces / depth precision: consistent with diagonals aligned to mesh triangles and artifacts that change with camera zoom. Ringworld's extreme scale adds risk, but ordinary games can also show it. NVIDIA explains depth quantization and transform roundoff: https://developer.nvidia.com/blog/visualizing-depth-precision/
2. Shadow acne or bias problems: particularly plausible for dark terrain stripes in vanilla. Unity documents how finite shadow-map resolution and bias cause self-shadowing artifacts: https://docs.unity3d.com/Manual/ShadowPerformance.html
3. Graphics driver or shader compatibility: plausible with an older Unity game on Adreno/Windows on Arm. Windows emulates x64 applications; this does not prove the emulator or GPU driver caused these specific images. Platform background: https://learn.microsoft.com/en-ca/windows/arm/apps-on-arm-x86-emulation
4. Memory pressure or physical hardware fault: not demonstrated. Low memory can cause allocation failures or stalls, but these pictures alone cannot identify it. A damaged GPU is not a justified conclusion from this evidence.

Useful controlled checks, without changing multiple settings at once:

- Reproduce at the same stock Kerbin location and camera angle in a clean instance. Temporarily disable celestial-body self shadows / shadows, then restore. If the stripes disappear, prioritize shadow rendering.
- Change only terrain shader quality, restart KSP, and compare. Then separately compare anti-aliasing off versus the original value. Record results rather than assuming either is the fix.
- Compare current OEM-approved graphics/firmware updates with the installed driver version. Do not install a random vendor's GPU driver.
- Watch memory and GPU shared-memory use while the artifact actually occurs, and retain KSP.log/Player.log for allocation or device errors. Reducing texture quality is a memory-pressure comparison, not an established cure.
- Reproduction in unrelated games or persistent corruption outside KSP would increase suspicion of a broader driver/hardware problem. A KSP-only, camera-dependent pattern favors its rendering path.

No system drivers or permanent stock graphics settings were changed for this diagnosis.
