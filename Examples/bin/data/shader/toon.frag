// http://www.lighthouse3d.com/opengl/glsl/index.php?toon3
varying vec3 normal;
varying vec4 vertex;

void main()
{
        float intensity;
        vec4 color;
        vec3 n = normalize(normal);

        vec3 v= vec3( gl_LightSource[0].position - vertex );
        intensity = dot(v, n);

        if (intensity > 0.95)
                color = vec4(1.0, 1.0, 1.0, 1.0);
        else if (intensity > 0.5)
                color = vec4(0.6, 0.6, 0.6, 1.0);
        else if (intensity > 0.25)
                color = vec4(0.4, 0.4, 0.4, 1.0);
        else
                color = vec4(0.1, 0.1, 0.1, 1.0);
        
        gl_FragColor = color;
}
