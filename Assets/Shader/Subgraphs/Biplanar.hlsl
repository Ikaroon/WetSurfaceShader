// Biplanar mapping shader originally written by Inigo Quilez
// https://iquilezles.org/www/articles/biplanar/biplanar.htm

// Biplanar mapping
void Biplanar_float(Texture2D t, SamplerState s, float3 worldPosition, float3 worldNormal, float blending, out float4 output)
{
	float3 p = worldPosition;
	float3 n = abs(worldNormal);
	
	// Coordinate derivatives for texturing
	float3 dpdx = ddx(p);
	float3 dpdy = ddy(p);

	// Major axis (in x; yz are following axis)
	uint3 ma = (n.x > n.y && n.x > n.z) ? uint3(0, 1, 2) :
			   (n.y > n.z             ) ? uint3(1, 2, 0) :
										  uint3(2, 0, 1) ;

	// Minor axis (in x; yz are following axis)
	uint3 mi = (n.x < n.y && n.x < n.z) ? uint3(0, 1, 2) :
			   (n.y < n.z             ) ? uint3(1, 2, 0) :
										  uint3(2, 0, 1) ;

	// Median axis (in x; yz are following axis)
	uint3 me = 3 - mi - ma;

	ma.yz = (ma.x == 0 ? ma.zy : ma.yz);
	me.yz = (me.x == 0 ? me.zy : me.yz);

	// Project + fetch
	float4 x = SAMPLE_TEXTURE2D_GRAD(t, s,
									float2(   p[ma.y],    p[ma.z]),
									float2(dpdx[ma.y], dpdx[ma.z]),
									float2(dpdy[ma.y], dpdy[ma.z]));

	float4 y = SAMPLE_TEXTURE2D_GRAD(t, s,
									float2(   p[me.y],    p[me.z]),
									float2(dpdx[me.y], dpdx[me.z]),
									float2(dpdy[me.y], dpdy[me.z]));

	// Blend factors
	float2 w = float2(n[ma.x], n[me.x]);
	// Make local support
	w = saturate((w - 0.5773) / (1 - 0.5773));
	// Shape transition
	w = pow(w, blending);

	// Blending
	output = (x * w.x + y * w.y) / (w.x + w.y);
}
