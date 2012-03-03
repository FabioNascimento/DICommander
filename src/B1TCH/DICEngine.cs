using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using SAPbobsCOM;

namespace DIC
{
    public class DICEngine
    {
        private static DICEngine instance=null;
        private static bool initialized = false;
        private static bool autorelease = true;

        //public System.Collections.Generic.SortedDictionary<string, CompanyClass> Sessions;
        public System.Collections.ArrayList Sessions;
        public SessionContainer DefaultSession = null;
       
        private DICEngine()
        {
            Sessions= new ArrayList();
            initialized = true;
        }

        private static void init()
        {
            if(!initialized)
            {
                instance=new DICEngine();
            }
        }

        public static DICEngine GetInstance()
        {
            init();            
            return instance;
        }

        public static bool AutoRelease
        {
            get
            {
                init();
                return autorelease;
            }
            set
            {
                init();
                autorelease = value;
            }
        }
    }

    public class SessionContainer
    {
        public string Handle;
        public CompanyClass Session;
        public override string ToString()
        {
            return Handle+" ("+((Session!=null && Session.Connected)?Session.CompanyName:"NULL")+")";
        }

        public override bool Equals(object obj)
        {
            return Handle.Equals(obj);
        }
    }
}
