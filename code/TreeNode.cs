using System;
using System.Net.Http;

namespace AdaptiveCache
{
    class TreeNode
    {
        public TreeNode left;
        public TreeNode right;
        public object Value;
        public int Key;
        public int TTL;
        public DateTime LastUpdated;

        public TreeNode (int key, object value = null)
        {
            this.Key = key;
            this.Value = value;
            this.TTL = CONSTANTS.TimeToLive;
            this.LastUpdated = DateTime.Now;
        }
    }
}
