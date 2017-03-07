namespace PaintDotNet
{
    using System;
    using System.Collections.Generic;

    internal static class PersistedObjectLocker
    {
        private static Dictionary<Guid, WeakReference> guidToPO = new Dictionary<Guid, WeakReference>();

        public static Guid Add<T>(PersistedObject<T> po)
        {
            Guid key = Guid.NewGuid();
            WeakReference reference = new WeakReference(po);
            guidToPO.Add(key, reference);
            return key;
        }

        public static PersistedObject<T> Get<T>(Guid guid)
        {
            WeakReference reference;
            guidToPO.TryGetValue(guid, out reference);
            if (reference == null)
            {
                return null;
            }
            object target = reference.Target;
            if (target == null)
            {
                guidToPO.Remove(guid);
                return null;
            }
            return (PersistedObject<T>) target;
        }

        public static void Remove(Guid guid)
        {
            guidToPO.Remove(guid);
        }
    }
}

