// Scaled-space depth can quantize the near hull and far inner surface together.
// Resolve that occlusion analytically rather than drawing through the outer hull.
bool ringHullOccludes(float3 camera,float3 surfacePoint,float radius,float halfWidth)
{
    if(radius<=0)return false;
    float3 c=camera/radius,p=surfacePoint/radius;
    float radial=dot(c.xz,c.xz);
    if(radial<=1)return false;
    // Every ray from this region to the interior must cross the hull band.
    if(abs(camera.y)<=halfWidth)return true;
    float3 d=p-c;float a=dot(d.xz,d.xz),b=dot(c.xz,d.xz);
    float discriminant=b*b-a*(radial-1);
    if(a<1e-12||discriminant<0)return false;
    float t=(-b-sqrt(discriminant))/a;
    return t>0&&t<.9999&&abs(c.y+d.y*t)<halfWidth/radius;
}
