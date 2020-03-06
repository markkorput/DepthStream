using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace FuseTools {
    public class KeyInterface<TypeT, KeyT> where TypeT : new() {
        private System.Action<TypeT,KeyT> assignFunc;
        private System.Func<TypeT,KeyT,bool> compFunc;
        private List<TypeT> list;

        public KeyInterface(List<TypeT> list, System.Action<TypeT,KeyT> assignFunc, System.Func<TypeT,KeyT,bool> compFunc) {
            this.list = list;
            this.assignFunc = assignFunc;
            this.compFunc = compFunc;
        }

        public TypeT Find(KeyT key) {
            return this.list.FirstOrDefault((item) => this.compFunc.Invoke(item, key));
        }

        public TypeT FindOrCreate(KeyT key) {
            var item = Find(key);
            if (item == null)
                item = Create(key);
            return item;
        }

        public TypeT FindOrCreate(KeyT key, System.Action<TypeT> afterCreateCallback) {
            var item = Find(key);
            if (item == null) {
                item = Create(key);
                afterCreateCallback.Invoke(item);
            }
            return item;
        }

        private TypeT Create(KeyT key) {
            var item = new TypeT();
            this.assignFunc.Invoke(item, key);
            this.list.Add(item);
            return item;
        }

        public void Delete(KeyT key, System.Action<TypeT> func = null) {
            var removes = this.list.FindAll((item) => this.compFunc.Invoke(item, key));
            foreach(var item in removes) {
                this.list.Remove(item);
                if (func != null) func.Invoke(item);
            }
        }
    }
}
