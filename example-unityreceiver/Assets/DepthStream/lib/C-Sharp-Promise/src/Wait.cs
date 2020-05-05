using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RSG
{
    public static class Wait
    {
        public static IEnumerator WhilePending(IPromise p) {
            bool isPending = true;
            p.Finally(() => isPending = false);
            return new WaitWhile(() => isPending);
        }
    }
}
