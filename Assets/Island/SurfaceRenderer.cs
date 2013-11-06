using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
//using System.Windows.Media.Imaging;
using UnityEngine;

public static class H
{
    public static Vector3 Convert(Vector3 v)
    {
        return new Vector3(v.x,v.z,v.y);
    }

    public static Vector4 Convert(Vector4 v)
    {
        return new Vector4(v.x,v.z,v.y,v.w);
    }

    public static void Convert(Matrix4x4 from, out Matrix4x4 to)
    {
        to = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            to.SetColumn(i,Convert(from.GetColumn(i)));
    }

    public static Vector4 float4(float x, float y, float z, float w) { return new Vector4(x, y, z, w); }
    public static Vector4 float4(float v) { return new Vector4(v, v, v, v); }
    public static Vector4 float4(Vector3 xyz, float w) { return new Vector4(xyz.x, xyz.y, xyz.z, w); }
    public static Vector3 float3(float v) { return new Vector3(v, v, v); }
    public static Vector3 float3(float x, float y, float z) { return new Vector3(x, y, z); }
    public static Vector2 float2(float x, float y) { return new Vector2(x, y); }
    public static Vector2 float2(float v) { return new Vector2(v, v); }

}


public struct Range
{
    public float Min, 
                Max;

    public float Center { get { return (Min + Max) / 2.0f; } set { float extend = Extend; Min = value - extend / 2.0f; Max = value + extend / 2.0f; } }
    public float Extend { get { return Max - Min; } set { float center = Center; Min = center - value / 2.0f; Max = center + value / 2.0f; } }
    public bool IsValid { get { return Max >= Min; } }

    public Range(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public void SetCenter(float center, float radius)
    {
        Min = center - radius;
        Max = center + radius;
    }

    public bool Contains(float v)
    {
        return v >= Min && v <= Max;
    }
    public void Include(float v)
    {
        Min = Mathf.Min(v, Min);
        Max = Mathf.Max(v, Max);
    }
    public void Include(Range r)
    {
        Min = Mathf.Min(r.Min, Min);
        Max = Mathf.Max(r.Max, Max);
    }

    public bool Intersects(Range r)
    {
        return r.Max >= Min && r.Min <= Max;
    }

    public override string ToString()
    {
        return "[" + Min + ", " + Max + "]";
    }

    public float Derelativate(float x)  {return Min + (Max - Min) * x;}
    public float Relativate(float x)  {return (x-Min) / (Max - Min);}
}

public struct Rect
{
    public Range x,y;
    public float Area { get { return x.Extend * y.Extend; } }
    public bool IsValid { get { return x.IsValid && y.IsValid; } }
    public Vector2 Center { get { return new Vector2(x.Center, y.Center); } }
    public Vector2 Min { get { return new Vector2(x.Min, y.Min); } set { x.Min = value.x; y.Min = value.y; } }
    public Vector2 Max { get { return new Vector2(x.Max, y.Max); } set { x.Max = value.x; y.Max = value.y; } }




    public Rect(Vector2 min, Vector2 max)
    {
        x = new Range(min.x,max.x);
        y = new Range(min.y,max.y);
    }

    public float Width {get {return x.Extend;}}
    public float Height {get {return y.Extend;}}


    public void SetCenter(float centerX, float centerY, float radiusX, float radiusY)
    {
        x.SetCenter(centerX, radiusX);
        y.SetCenter(centerY, radiusY);
    }

    public Range this[int axis]
    {
        get
        {
            switch (axis)
            {
                case 0:
                    return x;
                case 1:
                    return y;
            }
            throw new Exception("Unexpected index for Rect[]");
        }
        set
        {
            switch (axis)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                default:
                    throw new Exception("Unexpected index for Rect[]");
            }
        }
    }

    public bool Contains(Vector2 coords)
    {
        return x.Contains(coords.x) && y.Contains(coords.y);
    }

    public void Include(Vector2 coords)
    {
        x.Include(coords.x);
        y.Include(coords.y);
    }

    public void Include(Rect r)
    {
        x.Include(r.x);
        y.Include(r.y);
    }

    public override string ToString()
    {
        return "[ (" + x.Min + ", " + y.Min + "), (" + x.Max + ", " + y.Max + ") ]";
    }

    public void Derelativate(float x, float y, out float outX, out float outY)
    {
        outX = this.x.Derelativate(x);
        outY = this.y.Derelativate(y);
    }
    public void Relativate(float x, float y, out float outX, out float outY)
    {
        outX = this.x.Relativate(x);
        outY = this.y.Relativate(y);
    }

}

public struct Box
{
    public Range x, y, z;
    public float Volume { get { return x.Extend * y.Extend * z.Extend; } }
    public bool IsValid { get { return x.IsValid && y.IsValid && z.IsValid; } }
    public Vector3 Center { get { return new Vector3(x.Center, y.Center, z.Center); } }
    public Vector3 Min { get { return new Vector3(x.Min, y.Min, z.Min); } set { x.Min = value.x; y.Min = value.y; z.Min = value.z; } }
    public Vector3 Max { get { return new Vector3(x.Max, y.Max, z.Max); } set { x.Max = value.x; y.Max = value.y; z.Max = value.z; } }

    public Box(Range r)
    {
        x = r;
        y = r;
        z = r;
    }

    public Box(Vector3 min, Vector3 max)
    {
        x = new Range(min.x,max.x);
        y = new Range(min.y,max.y);
        z = new Range(min.z,max.z);
    }

    public override string ToString()
    {
        return "[ (" + x.Min + ", " + y.Min + ", " + z.Min + "), (" + x.Max + ", " + y.Max + ", " + z.Max + ") ]";
    }


    public Range this[int axis]
    {
        get
        {
            switch (axis)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
            }
            throw new Exception("Unexpected index for Box[]");
        }
        set
        {
            switch (axis)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
                default:
                    throw new Exception("Unexpected index for Box[]");
            }
        }
    }

    public bool Contains(Vector3 coords)
    {
        return x.Contains(coords.x) && y.Contains(coords.y) && z.Contains(coords.z);
    }

    public void Include(Vector3 coords)
    {
        x.Include(coords.x);
        y.Include(coords.y);
        z.Include(coords.z);
    }
    public void Include(Box box)
    {
        x.Include(box.x);
        y.Include(box.y);
        z.Include(box.z);
    }

    public int MaxAxis()
    {
        float xExt = x.Extend,
                yExt = y.Extend,
                zExt = z.Extend;
        if (xExt > yExt)
        {
            if (xExt > zExt)
                return 0;
            return 2;
        }
        if (yExt > zExt)
            return 1;
        return 2;
    }

    public bool Intersects(Box box)
    {
        return x.Intersects(box.x) && y.Intersects(box.y) && z.Intersects(box.z);
    }


    public static Box Wrap(Box box0, Box box1)
    {
        box0.Include(box1);
        return box0;
    }
}


struct Three<T>
{
    public T    a,b,c;

    public T this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                case 2:
                    return c;
            }
            return a;
        }
        set
        {

            switch (i)
            {
                case 0:
                    a = value;
                break;
                case 1:
                    b = value;
                break;
                case 2:
                    c = value;
                break;
            }

        }
    }
};


struct Two<T>
{
    public T    a,b;

    public T this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
            }
            return a;
        }
        set
        {

            switch (i)
            {
                case 0:
                    a = value;
                break;
                case 1:
                    b = value;
                break;
            }

        }
    }
};




struct Sphere
{
    public Vector3      Center;
    public Vector3 UnityCenter;
    public float        Radius;

    public void Finish()
    {
        Radius = Mathf.Sqrt(Radius);
        UpdateUnityCenter();
    }

    public void UpdateUnityCenter()
    {
        UnityCenter = H.Convert(Center);
    }
    public Sphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
        UnityCenter = H.Convert(center);
    }


    public void IncludeQuadratic(Vector3 p)
    {
        Vector3 d = p - Center;
        float d2 = Vector3.Dot(d,d);
        Radius = Mathf.Max(Radius,d2);
    }
};

class Frustum
{
    private Plane[] planes;
    public Vector3  UnityCenter {get; private set;}
    public Vector3  Center { get; private set; }


    public Frustum(Camera source)
    {
		planes = GeometryUtility.CalculateFrustumPlanes(source);
        UnityCenter = source.ViewportToWorldPoint(new Vector3(0.5f,0.5f,0));
        Center = H.Convert(UnityCenter);
    }

    public float GetDistanceTo(Vector3 p)
    {
        return Vector3.Distance(p,Center);
    }

    public bool IsVisible(Sphere sphere)
    {
        return true;
        for (int i = 0; i < planes.Length; i++)
            if (planes[i].GetDistanceToPoint(sphere.UnityCenter) < -sphere.Radius*1.1f)
                return false;
        return true;
    }
}


class Cell
{
    public const int    PhysicalResolution = 8,
                        VisualResolution = 32;
    public Three<bool>  is_open;
    public bool		    force_subdivision,
						TopIsVisible,
						BottomIsVisible,
						WaterIsVisible,
						processed,
						is_subdivided;
	Three<Cell>		    neighbor;
    Cell				Parent;
	SurfaceRenderer	    island;
	Two<Cell>	        child;
	Vector2				coords;
	int					shape;
	float				scale;
	uint				depth;
	uint				unique_id;
	Three<Sphere>       boundingSphere;
	List<Vector3>	    vertices=new List<Vector3>();
	//Buffer<MyTree,0>	trees;
	Three<Matrix4x4>    matrix,
                        unityMatrix;
	float				edge_length;

    private static void transform(Matrix4x4 m, ref Vector3 v)
    {
        v= m.MultiplyPoint3x4(v);
    }
    private static Vector4 float4(float x, float y, float z, float w)   {return new Vector4(x,y,z,w);}
    private static Vector4 float4(float v) { return new Vector4(v, v, v, v); }
    private static Vector4 float4(Vector3 xyz, float w) { return new Vector4(xyz.x, xyz.y, xyz.z, w); }
    private static Vector3 float3(float v)  {return new Vector3(v,v,v);}
    private static Vector3 float3(float x, float y, float z) { return new Vector3(x, y, z); }
    private static Vector2 float2(float x, float y) { return new Vector2(x, y); }
    private static Vector2 float2(float v) { return new Vector2(v, v); }


    private static float QuadraticDistance(Vector3 u, Vector3 v)    {Vector3 d = u - v; return Vector3.Dot(d,d);}
	public Cell()
    {
        depth = 0;
	    edge_length = 0;
	    force_subdivision = false;
	    processed = false;
	    is_subdivided = false;
	    TopIsVisible = false;
	    BottomIsVisible = false;
	    WaterIsVisible = false;
	    is_open[0] = false;
	    is_open[1] = false;
	    is_open[2] = false;
	    //unique_id = cell_id_counter++;
    }

	public void				Set(Cell n0, Cell n1, Cell n2)
    {
	    neighbor[0] = n0;
	    neighbor[1] = n1;
	    neighbor[2] = n2;
	    Parent = null;
	    child[0] = null;
	    child[1] = null;
	    is_open[0] = n0==null;
	    is_open[1] = n1==null;
	    is_open[2] = n2==null;
    }


	private static List<Vector3> water_vertices = new List<Vector3>();


	public void				Create(SurfaceRenderer owner, int shape, float scale, Vector2 coords, Vector3 world_pos)
    {
	    island = owner;
	    this.shape = shape;
	    this.scale = scale;
	    this.coords = coords;
	    //this.world_pos = world_pos;

	    float scale0 = scale / 2.0f;//(float)sqrt(2.f);
        Vector4 x = float4(0),y = float4(0),w = float4(0);
	    switch (shape)
	    {
		    case 0:
			    x = float4(scale,0,0,0);
			    y = float4(0,scale,0,0);
			    w = float4(coords.x-scale*0.5f,coords.y-scale*0.5f,0,1);
		    break;
		    case 1:
			    x = float4(-scale,0,0,0);
			    y = float4(0,-scale,0,0);
			    w = float4(coords.x+scale*0.5f,coords.y+scale*0.5f,0,1);
		    break;
		    case 2:
			    x = float4(0,-scale,0,0);
			    y = float4(scale,0,0,0);
			    w = float4(coords.x-scale*0.5f,coords.y+scale*0.5f,0,1);
		    break;
		    case 3:
			    x = float4(0,scale,0,0);
			    y = float4(-scale,0,0,0);
			    w = float4(coords.x+scale*0.5f,coords.y-scale*0.5f,0,1);
		    break;

		    case 4:
			    x = float4(-scale0,scale0,0,0);
			    y = float4(-scale0,-scale0,0,0);
			    w = float4(coords.x+scale*0.5f,coords.y,0,1);
		    break;
		    case 5:
			    x = float4(-scale0,-scale0,0,0);
			    y = float4(scale0,-scale0,0,0);
			    w = float4(coords.x,coords.y+scale*0.5f,0,1);
		    break;
		    case 6:
			    x = float4(scale0,-scale0,0,0);
			    y = float4(scale0,scale0,0,0);
			    w = float4(coords.x-scale*0.5f,coords.y,0,1);
		    break;
		    case 7:
			    x = float4(scale0,scale0,0,0);
			    y = float4(-scale0,scale0,0,0);
			    w = float4(coords.x,coords.y-scale*0.5f,0,1);
		    break;
	    }
        island.DbgShape = shape;
        island.DbgX = x;
        island.DbgY = y;
	    //owner.last_extend.derelativate(m.x.x * 0.5f + 0.5f,m.x.y * 0.5f+0.5f,m.x.x,m.x.y);
	    x.x *= owner.HalfWidth;
	    x.y *= owner.HalfHeight;
	    y.x *= owner.HalfWidth;
	    y.y *= owner.HalfHeight;
        island.DbgHalfWidth = owner.HalfWidth;
        island.DbgHalfHeight = owner.HalfHeight;

        Matrix4x4   m = new Matrix4x4();
        m.SetColumn(0, x);
        m.SetColumn(1, y);
	    //Vec::mult(m.x.xyz,owner.last_extend.width() / 2.f);
	    //Vec::mult(m.y.xyz,owner.last_extend.height() / 2.f);
	    m.SetColumn(2, float4(0,0,1,0));
        float wx,wy;
	    owner.Derelativate(w.x * 0.5f + 0.5f,w.y * 0.5f+0.5f,out wx,out wy);
        w.x = wx;
        w.y = wy;
        m.SetColumn(3, w);




	    //Vec::mult(m.w.xyz,map_extend);


	    vertices.Clear();

	    Vector3	center = new Vector3(0,0,0);

	    if (depth < 2)
	    {
#if false
            Random.seed = m.GetHashCode();
		    
            for (uint row = 0; row <= plantResolution; row++)
			    for (uint column = 0; column <= row; column++)
			    {
				    TVector3<>	in;
				    in.x = float(column) / float(plantResolution);
				    in.y = 1.f-float(row)/float(plantResolution);
				    in.z = 0.f;
				    Mat::transform(m,in);
				    //std::swap(in.y,in.z);
				    //in.z *= -1.f;
				    float normal_z,waterHeight;
				    if ((row%2) || (column%2))
				    {
					    if (owner.sampleSurfaceAt(in.xy,in.z,normal_z,waterHeight))
					    {
						    float plant_probability = (1.0 - cubicStep(waterHeight,0.0f,0.2f));
		
						    float tree_probability = cubicStep(normal_z,0.8f,1.0f) * plant_probability;
						    if (random.getFloat() < tree_probability * 0.75f)
						    {
							    MyTree&t = trees.append();
							    t.position = in;
							    t.rendered_to_map = false;
							    owner.tree_map_changed = true;
						    }

					    }
				    }
			    }
#endif
        }

	    for (uint row = 0; row <= PhysicalResolution; row++)
		    for (uint column = 0; column <= row; column++)
		    {
			    Vector3	p;
			    p.x = (float)(column) / (float)(PhysicalResolution);
			    p.y = 1.0f-(float)(row)/(float)(PhysicalResolution);
			    p.z = 0.0f;
			    transform(m,ref p);
			    //std::swap(in.y,in.z);
			    //in.z *= -1.f;
			    p.z = owner.SampleHeightAt(p.x,p.y);
			    vertices.Add(p);
			    center += p;
		    }
	    vertices.TrimExcess();
	    //vbo.loadStructs(vertices);
	    center /= (float)vertices.Count;

	    edge_length = 0;
	    int at = 0;
	    for (int i = 1; i <= PhysicalResolution;i++)
	    {
		    int next = at+i+1;
		    edge_length += Vector3.Distance(vertices[next],vertices[at]);
		    at = next;
	    }
        island.DbgEdgeLength = edge_length;
        island.DbgMatrix = m;
	 
	    boundingSphere.a = new Sphere(center,0.0f);

	    for (int i = 0; i < vertices.Count; i++)
            boundingSphere.a.IncludeQuadratic(vertices[i]);
        boundingSphere.a.Finish();

        boundingSphere.b = boundingSphere.a;
        boundingSphere.b.Center.z = owner.SampleLowerHeightAt(boundingSphere.b.Center.x, boundingSphere.b.Center.y);
        boundingSphere.b.UpdateUnityCenter();

        center = float3(0);

	    water_vertices.Clear();
	    for (int row = 0; row <= PhysicalResolution; row++)
		    for (int column = 0; column <= row; column++)
		    {
			    Vector3 p;
			    p.x = (float)(column) / (float)(PhysicalResolution);
			    p.y = 1.0f-(float)(row)/(float)(PhysicalResolution);
			    p.z = 0.0f;
			    transform(m,ref p);
			    //std::swap(in.y,in.z);
			    //in.z *= -1.f;
			    p.z = owner.SampleWaterHeightAt(p.x, p.y);
			    water_vertices.Add(p);
			    center += p;
		    }
        boundingSphere.c.Center = center;
        boundingSphere.c.Radius = 0;
	    for (int i = 0; i < water_vertices.Count; i++)
            boundingSphere.c.IncludeQuadratic(water_vertices[i]);
        boundingSphere.c.Finish();


        for (int i = 0; i < 3; i++)
        {
            m.SetColumn(2, float4(0, 0, boundingSphere[i].Radius, 0));
            w = m.GetColumn(3);
            w.z = boundingSphere[i].Center.z;
            m.SetColumn(3, w);
            matrix[i] = m;

            Matrix4x4 um;
            H.Convert(m, out um);
            unityMatrix[i] = um;
        }


	    //lower_bounding_sphere.center.z
	    //Vec::center(v0,v1,v2,bounding_sphere.center);
	    //bounding_sphere.radius = vsqrt(vmax(vmax(Vec::quadraticDistance(v0,bounding_sphere.center),Vec::quadraticDistance(v1,bounding_sphere.center)),Vec::quadraticDistance(v2,bounding_sphere.center)));


	    //for (index_t i = 0; i < vertices.count(); i++)
	    //	ASSERT_LESS1__(Vec::distance(bounding_sphere.center,vertices[i]),bounding_sphere.radius*1.01f,i);

    }
	public void				Create(SurfaceRenderer owner, int shape, float scale, Vector2 coords)
    {
	    Vector3	world_pos;
		
	    owner.Relativate(coords.x * 0.5f + 0.5f,coords.y * 0.5f + 0.5f,out world_pos.x,out world_pos.y);
	    world_pos.z = owner.SampleHeightAt(world_pos.x,world_pos.y);
	    Create(owner,shape,scale,coords,world_pos);
    }

	public bool				Process(Frustum f)
    {
    	TopIsVisible |=	f.IsVisible(boundingSphere.a);
        BottomIsVisible |= f.IsVisible(boundingSphere.b);
        WaterIsVisible |= f.IsVisible(boundingSphere.c);
	    processed = true;
	    if (!force_subdivision && !TopIsVisible)
		    return false;
	    //Cell&cell0 = cells[sector_index];
	    //cell0.coords.x = x;
	    //cell0.coords.y = y;
	    //cell0.shape = shape;
	    float edge_len = edge_length;
	    //float dbg_len = (shape > 3 ? 1.f : (float)sqrt(2.f)) * map_extend * scale;
	    //TVec3<>	world_pos = {coords.x * map_extend, 0.f, coords.y*map_extend};
	    //world_pos.y = resolveHeightAt(world_pos.x,world_pos.z);
	    float distance = Mathf.Max(f.GetDistanceTo(boundingSphere.a.Center)-edge_len*0.5f,0.0f);
        island.DbgDistance = distance;
	    bool result = false;
	    bool edge_is_open = is_open[0];
	    Cell	n0 = neighbor[0];
	    bool opposing_exists = n0!= null && n0.processed;
	    bool opposing_subdivided = opposing_exists && n0.is_subdivided;
		    //n0.child[0];
	    bool want_to_subdivide = (this.depth<2 ||  distance < edge_len) && edge_len > 4.0f;
	    bool force_subdivide = opposing_subdivided || force_subdivision;
	    if (want_to_subdivide || force_subdivide)
	    {
				
		    if ((want_to_subdivide || force_subdivide) && !opposing_exists && !edge_is_open && Parent!=null)
		    {
			    //walk up until seeing neighbor
			    Cell	child = this,
					    parent = this.Parent;
			    if (n0 != null)
			    {
				    n0.setForceSubdivision();
				    //ASSERT__(!n0.is_subdivided);
				    //OutputDebugStringA((String(unique_id)+" requests children of "+String(n0.unique_id)+"\n").c_str());
				    result = true;
			    }
			    else
				    for(;;)
				    {
					    int edge0;
					    {
						    if (parent.child[0] == child)
							    edge0 = 1;
						    else
							    edge0 = 2;
						    if (parent.is_open[edge0])
							    break;
						    Cell	n = parent.neighbor[edge0];
						    if (n != null)
						    {
							    n.setForceSubdivision();
							    //ASSERT__(!n.is_subdivided);
							    //n.force_subdivision = true;
							    //OutputDebugStringA((String(unique_id)+" requests children of "+String(n.unique_id)+"\n").c_str());
							    result = true;
							    break;
						    }
						    child = parent;
						    parent = parent.Parent;
						    if (parent == null)
							    break;
					    }
					    {
						    //const Cell&cell = cells[parent];
						    if (parent.is_open[0])
							    break;
						    Cell	n = parent.neighbor[0];
						    if (n != null)
						    {
							    n.setForceSubdivision();
							    //ASSERT__(!n.is_subdivided);
							    //OutputDebugStringA((String(unique_id)+" requests children of "+String(n.unique_id)+"\n").c_str());
							    //n.force_subdivision = true;
							    result = true;
							    break;
						    }
						    child = parent;
						    parent = parent.Parent;
						    if (parent != null)
							    break;
					    }
				    }
			    }
		    if ((want_to_subdivide || force_subdivide) && (opposing_exists || edge_is_open) )
		    //if (((want_to_subdivide  ) || force_subdivide) && (shape == 1))
		    //if (false)
		    //if (this.depth < 3)
		    {
			    if (child[0] == null)
			    {
				    child[0] = new Cell();
				    child[1] = new Cell();

				    child[0].neighbor[1] = child[1];
				    child[1].neighbor[2] = child[0];
				    child[0].Parent = this;
				    child[1].Parent = this;
				    child[0].depth = depth+1;
				    child[1].depth = depth+1;
				    SurfaceRenderer owner = island;
				    //ASSERT__(owner);
				    switch (shape)
				    {
					    case 0:
						    child[0].Create(owner,4,scale,float2(coords.x-scale*.5f,coords.y));
						    child[1].Create(owner,5,scale,float2(coords.x,coords.y-scale*.5f));
					    break;
					    case 1:
						    child[0].Create(owner,6,scale,float2(coords.x+scale*.5f,coords.y));
						    child[1].Create(owner,7,scale,float2(coords.x,coords.y+scale*.5f));
					    break;
					    case 2:
						    child[0].Create(owner,7,scale,float2(coords.x,coords.y+scale*.5f));
						    child[1].Create(owner,4,scale,float2(coords.x-scale*.5f,coords.y));
					    break;
					    case 3:
						    child[0].Create(owner,5,scale,float2(coords.x,coords.y-scale*.5f));
						    child[1].Create(owner,6,scale,float2(coords.x+scale*.5f,coords.y));
					    break;
					    case 4:
						    child[0].Create(owner,2,scale*0.5f,float2(coords.x+scale*.25f,coords.y-scale*.25f));
						    child[1].Create(owner,0,scale*0.5f,float2(coords.x+scale*.25f,coords.y+scale*.25f));
					    break;
					    case 5:
						    child[0].Create(owner,0,scale*0.5f,float2(coords.x+scale*.25f,coords.y+scale*.25f));
						    child[1].Create(owner,3,scale*0.5f,float2(coords.x-scale*.25f,coords.y+scale*.25f));
					    break;
					    case 6:
						    child[0].Create(owner,3,scale*0.5f,float2(coords.x-scale*.25f,coords.y+scale*.25f));
						    child[1].Create(owner,1,scale*0.5f,float2(coords.x-scale*.25f,coords.y-scale*.25f));
					    break;
					    case 7:
						    child[0].Create(owner,1,scale*0.5f,float2(coords.x-scale*.25f,coords.y-scale*.25f));
						    child[1].Create(owner,2,scale*0.5f,float2(coords.x+scale*.25f,coords.y-scale*.25f));
					    break;
				    }

				    child[0].is_open[0] = is_open[1];
				    child[0].is_open[2] = is_open[0];

				    child[1].is_open[0] = is_open[2];
				    child[1].is_open[1] = is_open[0];

				    n0 = neighbor[0];
				    if (n0 != null)
				    {
					    if (n0.child[1]!=null)
					    {
						    child[0].neighbor[2] = n0.child[1];
						    n0.child[1].neighbor[1] = child[0];
					    }
					    if (n0.child[0]!=null)
					    {
						    child[1].neighbor[1] = n0.child[0];
						    n0.child[0].neighbor[2] = child[1];
					    }
				    }
				    Cell n1 = neighbor[1];
				    if (n1!=null)
				    {
					    if (n1.child[1]!=null)
					    {
						    child[0].neighbor[0] = n1.child[1];
						    n1.child[1].neighbor[0] = child[0];
					    }
				    }
				    Cell	n2 = neighbor[2];
				    if (n2!=null)
				    {
					    if (n2.child[0]!= null)
					    {
						    child[1].neighbor[0] = n2.child[0];
						    n2.child[0].neighbor[0] = child[1];
					    }
				    }
			    }
			    if (!is_subdivided)
				    result = true;
			    //Cell	cell = cells[sector_index];

			    is_subdivided = true;
			    result |= child[0].Process(f);
			    result |= child[1].Process(f);
			    //result |= opposing_exists && !opposing_subdivided;	//opposing must follow
		    }
	    }
	    return result;
    }
	public void				ResetPerFrameData()
    {
        force_subdivision = false;
	    TopIsVisible = false;
	    BottomIsVisible = false;
	    WaterIsVisible = false;
	    processed = false;
	    is_subdivided = false;

	    if (child[0] != null)
		    child[0].ResetPerFrameData();
	    if (child[1] != null)
		    child[1].ResetPerFrameData();
    }
	void				setForceSubdivision()
    {
	    if (force_subdivision || is_subdivided)
		    return;
	    force_subdivision = true;
	    if (Parent != null)
		    Parent.setForceSubdivision();
    }
	public void			RegisterVisibleLeaves(List<Cell> visible_leaves)
    {
	    if (!is_subdivided)
	    {
		    if (TopIsVisible || BottomIsVisible || WaterIsVisible)
			    visible_leaves.Add(this);
	    }
	    else
	    {
		    if (child[0] != null)
			    child[0].RegisterVisibleLeaves(visible_leaves);
		    if (child[1] != null)
			    child[1].RegisterVisibleLeaves(visible_leaves);
	    }
    }

#if false
	public bool			RetectEdgeIntersection(Vector3 b, Vector3 d)
    {
	    float dist = 1.f;
	    if (!Obj::detectOpticalSphereIntersection(bounding_sphere.center,bounding_sphere.radius,b,d,dist))
		    return false;

	    if (child[0] && child[1])
		    return child[0]->detectEdgeIntersection(b,d) || child[1]->detectEdgeIntersection(b,d);
	    dist = 1.f;
	    for (index_t i = 0; i < Rendering::physical_indices.count(); i+=3)
		    if (Obj::detectOpticalIntersection(vertices[Rendering::physical_indices[i]],vertices[Rendering::physical_indices[i+1]],vertices[Rendering::physical_indices[i+2]],b,d,dist))
			    return true;
	    return false;

    }
	bool				detectFirstIntersectedCell(const TVector3<>&b, const TVector3<>&d, float&distance, index_t&cell_id) const;
	void				detectAndHandleCollisions(const AbstractHull<>&hull);
#endif


    private static Mesh mesh;


    public static void StaticInit()
    {
        if (mesh != null)
            return;
        mesh = new Mesh();


        {
            List<Vector3> vertices = new List<Vector3>(),
                                normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();
			List<int>           indices = new List<int>();
		    //physical_indices.reset();
		    //visual_indices.reset();
		
            
		    for (int row = 0; row <= VisualResolution; row++)
			    for (int column = 0; column <= row; column++)
			    {
				    vertices.Add(float3((float)(column) / (float)(VisualResolution),1.0f-(float)(row)/(float)(VisualResolution),((vertices.Count%2) *2 -1)*10));
                    normals.Add(float3(0, 0, 1));
                    tangents.Add(float4(1, 0, 0, 1));
			    }


		    int thisRow = 0;
		    for (int row = 0; row < VisualResolution; row++)
		    {
			    int	thisRowLength = (row)+1,
					    nextRowLength = (row+1)+1;
			    int	nextRow = thisRow + thisRowLength;


			    indices.Add( thisRow+thisRowLength-1);
                indices.Add( nextRow+nextRowLength-2);
                indices.Add( nextRow+nextRowLength-1);

			    for (int quad = 0; quad < (thisRowLength-1); quad++)
			    {
				    if (((byte)(quad%2) ^ (byte)(row%2)) != 0)
				    {
					    indices.Add( thisRow + quad);
                        indices.Add( nextRow+quad);
                        indices.Add( thisRow+quad+1);

						indices.Add( nextRow+quad );
                        indices.Add( nextRow+quad+1 );
                        indices.Add( thisRow+quad+1 );
				    }
				    else
				    {
					    indices.Add( thisRow + quad);
                        indices.Add( nextRow+quad );
                        indices.Add( nextRow+quad+1);

						indices.Add( nextRow+quad+1);
                        indices.Add( thisRow+quad+1);
                        indices.Add( thisRow+quad);
				    }
			    }
			    thisRow = nextRow;
		    }
            indices.Reverse();
#if false
		    thisRow = 0;
		    for (index_t row = 0; row < physicalResolution; row++)
		    {
			    GLuint	thisRowLength = static_cast<GLuint>(row)+1,
					    nextRowLength = static_cast<GLuint>(row+1)+1;
			    GLuint	nextRow = thisRow + thisRowLength;


			    physical_indices << thisRow+thisRowLength-1 << nextRow+nextRowLength-2 << nextRow+nextRowLength-1;

			    for (GLuint quad = 0; quad < (thisRowLength-1); quad++)
			    {
				    if ((BYTE)(quad%2) ^ (BYTE)(row%2))
				    {
					    physical_indices	<< thisRow + quad << nextRow+quad << thisRow+quad+1
											    << nextRow+quad << nextRow+quad+1 << thisRow+quad+1;
				    }
				    else
				    {
					    physical_indices	<< thisRow + quad << nextRow+quad << nextRow+quad+1
											    << nextRow+quad+1 << thisRow+quad+1 << thisRow+quad;
				    }
			    }
			    thisRow = nextRow;
		    }

		    visual_indices.compact();
		    physical_indices.compact();
#endif
		    {
			    //ASSERT_EQUAL__(sizeof(float2),2*sizeof(float));
			    mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.tangents = tangents.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
             }


        }

    }

	public void			Render(int face)
    {
        Graphics.DrawMeshNow(mesh,unityMatrix[face]);
        //Graphics.DrawMesh(mesh, unityMatrix, material,0);
    }
    public void Render(Material material, int face)
    {
        //Graphics.DrawMeshNow(mesh, unityMatrix);
        Graphics.DrawMesh(mesh, unityMatrix[face], material,0);
    }	//void				putTrees(const Engine::Frustum<>&frustum, const TVector2<>&x);
	//void				rasterNewTrees();
};


class HeightMap
{
    private float[,] field;
    private Texture2D tex;
    public Texture2D Texture { get { return tex; } }


    public static Texture2D Load(string source)
    {
        byte[] bytes = ((TextAsset)Resources.Load(source, typeof(TextAsset))).bytes;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGB24, false, true);
        tex.LoadImage(bytes);
        return tex;
    }

    private static float DecodeHeight(Color32 c)
    {
        return ((float)c.r) / 255.0f + (float)c.g + (float)c.b * 255.0f - 1000.0f;
    }

    public HeightMap(string source)
    {
        tex = Load(source);
        Color32[] pixels = tex.GetPixels32();

        field = new float[tex.width,tex.height];
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                field[x, y] = DecodeHeight(pixels[y * tex.width + x]);
    }


    public float Sample(float u, float v)
    {
        u = Mathf.Clamp01(u) * (float)(tex.width-1);
        v = Mathf.Clamp01(v) * (float)(tex.height-1);
        int u0 = Mathf.FloorToInt(u);
        int u1 = Mathf.CeilToInt(u);
        int v0 = Mathf.FloorToInt(v);
        int v1 = Mathf.CeilToInt(v);
        float uf = u - (float)u0;
        float vf = v - (float)v0;

        float h00 = field[u0, v0],
                h10 = field[u1, v0],
                h11 = field[u1, v1],
                h01 = field[u0, v1];
        float h0 = Mathf.Lerp(h00, h01, vf),
            h1 = Mathf.Lerp(h10, h11, vf);
        return Mathf.Lerp(h0, h1, uf);
    }

};

[ExecuteInEditMode]
public class SurfaceRenderer : MonoBehaviour {

    private HeightMap h0,h1,h2;
    private Texture2D   topNormalMap,
                        bottomNormalMap;


    public Rect    lastExtend = new Rect();
    private Vector2 center;
    public Material TopMaterial,
                    BottomMaterial,
                    WaterMaterial;
    public bool     Initialized = false;
    
    public int DbgFrame = 0;
    public int DbgVisibleCells = 0,
                  DbgShape = -1;
    public Vector3 DbgCameraPosition;
    public Vector4 DbgX, DbgY;
    public float DbgDistance = -1.0f,
                DbgEdgeLength = -1,
                DbgHalfWidth0 = -1,
                DbgHalfHeight0 = -1,
                DbgHalfWidth = -1,
                DbgHalfHeight = -1;
    public Matrix4x4 DbgMatrix;

    public float HalfWidth { get { return lastExtend.x.Extend / 2.0f; } }
    public float HalfHeight { get { return lastExtend.y.Extend / 2.0f; } }
    public void Derelativate(float x, float y, out float outX, out float outY)
    {
        lastExtend.Derelativate(x, y, out outX, out outY);
    }
    public void Relativate(float x, float y, out float outX, out float outY)
    {
        lastExtend.Relativate(x, y, out outX, out outY);
    }




    public float SampleHeightAt(float x, float y)
    {
        return 0;
        //return h0.Sample(last_extend.x.Relativate(x), last_extend.y.Relativate(y));
    }
    public float SampleLowerHeightAt(float x, float y)
    {
        return 0;
        //return h1.Sample(last_extend.x.Relativate(x), last_extend.y.Relativate(y));
    }
    public float SampleWaterHeightAt(float x, float y)
    {
        return 0;
        //return h2.Sample(last_extend.x.Relativate(x), last_extend.y.Relativate(y));
    }


    void UpdateExtend(float centerX, float centerY, int mapWidth, int mapHeight)
    {
        center.x = centerX;
        center.y = centerY;
	    lastExtend.SetCenter(center.x,center.y,(float)(mapWidth)/4.0f/2.0f,(float)(mapHeight)/4.0f/2.0f);
        DbgHalfWidth0 = HalfWidth;
        DbgHalfHeight0 = HalfHeight;
    }


    void UpdateMaterial(Material m, Frustum f)
    {
        if (m == null)
            return;
        m.SetVector("cameraPosition", f.UnityCenter);
        m.SetVector("Region", new Vector4(lastExtend.x.Min, lastExtend.y.Min, lastExtend.x.Max, lastExtend.y.Max));
    }
    void UpdateMaterial(Material m, HeightMap h0, HeightMap h1, HeightMap h2, Texture2D normalMap)
    {
        if (m == null)
            return;
        m.SetTexture("_UpperHeightMap", h0.Texture);
        if (h1 != null)
            m.SetTexture("_LowerHeightMap", h1.Texture);
        if (h2 != null)
            m.SetTexture("_WaterHeightMap", h2.Texture);
        if (normalMap != null)
            m.SetTexture("_NormalMap", normalMap);
        m.SetVector("Region", new Vector4(lastExtend.x.Min, lastExtend.y.Min, lastExtend.x.Max, lastExtend.y.Max));
    }

    List<Cell>		visibleLeaves = new List<Cell>();

    Two<Cell>       root;

    public string DbgCamera;

    private Camera viewCamera;

    void OnRenderObject()
    {
        this.transform.position = new Vector3(0, Mathf.Sin(Time.time) * 0.5f, 0);
        viewCamera = Camera.current;
    }

    void ResetCells()
    {
        root.a = new Cell();
        root.b = new Cell();
        root.a.Set(root.b, null, null);
        root.b.Set(root.a, null, null);
        root.a.Create(this, 0, 2.0f, H.float2(0));
        root.b.Create(this, 1, 2.0f, H.float2(0));
    }

	void Start () {
        //if (Initialized)
          //  return;
        Initialized = true;
        Cell.StaticInit();

        if (h0 == null)
            h0 = new HeightMap("islandTop.png");
        if (h1 == null)
            h1 = new HeightMap("islandBottom.png");
        topNormalMap = HeightMap.Load("islandTopNormals.png");
        bottomNormalMap = HeightMap.Load("islandBottomNormals.png");
        // h1 = new HeightMap("islandBottom.png");
        //h2 = new HeightMap("islandWater.png");
        UpdateExtend(0, 0, h0.Texture.width, h0.Texture.height);

        ResetCells();
        DbgFrame = 0;
    }

    void OnGUI()
    {
        //DbgFrame++;
    }
	// Update is called once per frame
	void Update () {
        if (!Initialized || root.a == null)
            Start();

        //DbgCamera = Camera.current.name;
        
        root.a.ResetPerFrameData();
        root.b.ResetPerFrameData();
        visibleLeaves.Clear();

        if (viewCamera != null)
        {
            Frustum f = new Frustum(viewCamera);
            {
                for (; ; )
                {
                    //OutputDebugStringA("===== Next Iteration =====\n");
                    bool cont = false;
                    cont |= root.a.Process(f);
                    cont |= root.b.Process(f);
                    if (!cont)
                        break;
                }
            }
        }

        foreach (var camera in Camera.allCameras)
        {
            if (camera == viewCamera)
                continue;
            Frustum f = new Frustum(camera);
            {
                for (; ; )
                {
                    //OutputDebugStringA("===== Next Iteration =====\n");
                    bool cont = false;
                    cont |= root.a.Process(f);
                    cont |= root.b.Process(f);
                    if (!cont)
                        break;
                }
            }
        }

        //if (DbgFrame > 1000)
        //  return;
        DbgVisibleCells = visibleLeaves.Count;
        //DbgCameraPosition = f.UnityCenter;

        root.a.RegisterVisibleLeaves(visibleLeaves);
        root.b.RegisterVisibleLeaves(visibleLeaves);

        UpdateMaterial(TopMaterial,h0,h1,h2,topNormalMap);
        UpdateMaterial(BottomMaterial,h0,h1,h2,bottomNormalMap);
//        UpdateMaterial(WaterMaterial,h0,h1,h2);

        if (TopMaterial != null)
        {
            //TopMaterial.SetPass(0);
            foreach (var leaf in visibleLeaves)
                if (leaf.TopIsVisible)
                    leaf.Render(TopMaterial,0);
        }
        if (BottomMaterial != null)
        {
            //BottomMaterial.SetPass(0);
            foreach (var leaf in visibleLeaves)
                if (leaf.BottomIsVisible)
                    leaf.Render(BottomMaterial,1);
        }
        if (WaterMaterial != null)
        {
            //WaterMaterial.SetPass(0);
            foreach (var leaf in visibleLeaves)
                if (leaf.WaterIsVisible)
                    leaf.Render(WaterMaterial,2);
        }
        DbgFrame++;
	
	}
}
