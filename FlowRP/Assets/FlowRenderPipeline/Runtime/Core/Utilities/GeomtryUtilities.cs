namespace UnityEngine.Rendering.FlowPipeline
{
     /// <summary>
    /// Frustum class.
    /// </summary>
    public struct Frustum
    {
        /// <summary>
        /// Frustum planes.
        /// In order: left, right, top, bottom, near, far.
        /// </summary>
        public Plane[] planes;  // Left, right, top, bottom, near, far
        /// <summary>
        /// Frustum corner points.
        /// </summary>
        public Vector3[] corners; // Positions of the 8 corners

        static Vector3 IntersectFrustumPlanes(Plane p0, Plane p1, Plane p2)
        {
            Vector3 n0 = p0.normal;
            Vector3 n1 = p1.normal;
            Vector3 n2 = p2.normal;

            float det = Vector3.Dot(Vector3.Cross(n0, n1), n2);
            return (Vector3.Cross(n2, n1) * p0.distance + Vector3.Cross(n0, n2) * p1.distance - Vector3.Cross(n0, n1) * p2.distance) * (1.0f / det);
        }

        /// <summary>
        /// Creates a frustum.
        /// Note: when using a camera-relative matrix, the frustum will be camera-relative.
        /// </summary>
        /// <param name="frustum">Inout frustum.</param>
        /// <param name="viewProjMatrix">View projection matrix from which to build the frustum.</param>
        /// <param name="viewPos">View position of the frustum.</param>
        /// <param name="viewDir">Direction of the frustum.</param>
        /// <param name="nearClipPlane">Near clip plane of the frustum.</param>
        /// <param name="farClipPlane">Far clip plane of the frustum.</param>
        public static void Create(ref Frustum frustum, Matrix4x4 viewProjMatrix, Vector3 viewPos, Vector3 viewDir, float nearClipPlane, float farClipPlane)
        {
            GeometryUtility.CalculateFrustumPlanes(viewProjMatrix, frustum.planes);

            // We need to recalculate the near and far planes otherwise it does not work for oblique projection matrices used for reflection.
            Plane nearPlane = new Plane();
            nearPlane.SetNormalAndPosition(viewDir, viewPos);
            nearPlane.distance -= nearClipPlane;

            Plane farPlane = new Plane();
            farPlane.SetNormalAndPosition(-viewDir, viewPos);
            farPlane.distance += farClipPlane;

            frustum.planes[4] = nearPlane;
            frustum.planes[5] = farPlane;

            // Compute corners from the planes instead of projection matrix. Otherwise you get the same issue with near and far for oblique projection.
            frustum.corners[0] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[3], frustum.planes[4]);
            frustum.corners[1] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[3], frustum.planes[4]);
            frustum.corners[2] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[2], frustum.planes[4]);
            frustum.corners[3] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[2], frustum.planes[4]);
            frustum.corners[4] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[3], frustum.planes[5]);
            frustum.corners[5] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[3], frustum.planes[5]);
            frustum.corners[6] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[2], frustum.planes[5]);
            frustum.corners[7] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[2], frustum.planes[5]);
        }
    } // struct Frustum

}