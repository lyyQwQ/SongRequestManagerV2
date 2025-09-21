using System.Collections.Generic;
using UnityEngine;

namespace SongRequestManagerV2.Networks
{
    public class ImageCache : MonoBehaviour
    {
        private readonly object _lock = new object();

        private class Entry
        {
            public string Url;
            public Texture2D Tex;
        }

        private readonly Dictionary<string, LinkedListNode<Entry>> _map = new Dictionary<string, LinkedListNode<Entry>>();
        private readonly LinkedList<Entry> _lru = new LinkedList<Entry>();

        // 默认容量，可按需在实例化后调整
        public int Capacity = 500;

        public bool TryGet(string url, out Texture2D tex)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(url, out var node))
                {
                    // 提升为最近使用
                    _lru.Remove(node);
                    _lru.AddFirst(node);
                    tex = node.Value.Tex;
                    return tex != null;
                }
            }
            tex = null;
            return false;
        }

        public void Add(string url, Texture2D tex)
        {
            if (string.IsNullOrEmpty(url) || tex == null)
            {
                return;
            }

            lock (_lock)
            {
                if (_map.TryGetValue(url, out var existing))
                {
                    // 替换旧纹理，释放资源
                    if (existing.Value.Tex != null)
                    {
                        Destroy(existing.Value.Tex);
                    }
                    existing.Value.Tex = tex;
                    _lru.Remove(existing);
                    _lru.AddFirst(existing);
                }
                else
                {
                    var node = new LinkedListNode<Entry>(new Entry { Url = url, Tex = tex });
                    _lru.AddFirst(node);
                    _map[url] = node;
                }

                // 淘汰多余条目
                while (_map.Count > Capacity)
                {
                    var last = _lru.Last;
                    if (last == null) break;
                    _lru.RemoveLast();
                    _map.Remove(last.Value.Url);
                    if (last.Value.Tex != null)
                    {
                        Destroy(last.Value.Tex);
                    }
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var node in _lru)
                {
                    if (node.Tex != null)
                    {
                        Destroy(node.Tex);
                    }
                }
                _lru.Clear();
                _map.Clear();
            }
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}

