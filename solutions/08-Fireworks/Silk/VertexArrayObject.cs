using System;
using Silk.NET.OpenGL;

namespace Util;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
	where TVertexType : unmanaged
	where TIndexType : unmanaged
{
	private uint _handle;
	private GL _gl;
	private BufferObject<TVertexType> _vbo;
	private BufferObject<TIndexType> _ebo;

	public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
	{
		_gl = gl;
		_vbo = vbo;
		_ebo = ebo;

		_handle = _gl.GenVertexArray();
		Bind();
	}

	public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
		int offSet)
	{
		_gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType),
			(void*)(offSet * sizeof(TVertexType)));
		_gl.EnableVertexAttribArray(index);
	}

	public void Bind()
	{
		_gl.BindVertexArray(_handle);
		_vbo.Bind();
		_ebo.Bind();
	}

	public void Dispose()
	{
		_vbo.Dispose();
		_ebo.Dispose();
		_gl.DeleteVertexArray(_handle);
	}
}

public class VertexArrayObject<TVertexType> : IDisposable
	where TVertexType : unmanaged
{
	private uint _handle;
	private GL _gl;
	private BufferObject<TVertexType> _vbo;

	public VertexArrayObject (GL gl, BufferObject<TVertexType> vbo)
	{
		_gl = gl;
		_vbo = vbo;

		_handle = _gl.GenVertexArray();
		Bind();
	}

	public unsafe void VertexAttributePointer (uint index, int count, VertexAttribPointerType type, uint vertexSize,
		int offSet)
	{
		_gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType),
			(void*)(offSet * sizeof(TVertexType)));
		_gl.EnableVertexAttribArray(index);
	}

	public void Bind ()
	{
		_gl.BindVertexArray(_handle);
		_vbo.Bind();
	}

	public void Dispose ()
	{
		_vbo.Dispose();
		_gl.DeleteVertexArray(_handle);
	}
}
