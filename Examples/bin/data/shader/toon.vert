// http://www.lighthouse3d.com/opengl/glsl/index.php?toon3
varying vec3 normal;
varying vec4 vertex;

void main()
{
        vertex = gl_ModelViewMatrix * gl_Vertex;
        normal = gl_NormalMatrix * gl_Normal;
        gl_Position = ftransform();
}
