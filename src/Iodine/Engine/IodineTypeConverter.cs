using System;
using System.Collections.Generic;
using Iodine.Runtime;

namespace Iodine
{
	public class IodineTypeConverter
	{
		private static IodineTypeConverter _instance = null;

		public static IodineTypeConverter Instance {
			get {
				if (_instance == null) {
					_instance = new IodineTypeConverter ();
				}
				return _instance;
			}
		}

		private Dictionary<Type, ITypeConverter> conveters = new Dictionary<Type, ITypeConverter> ();

		public IodineTypeConverter ()
		{
			RegisterTypeConveter (typeof(Byte), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(Int16), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(UInt16), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(Int32), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(UInt32), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(Int64), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(UInt64), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(Boolean), new BoolTypeConverter ());
			RegisterTypeConveter (typeof(String), new StringTypeConverter ());
			RegisterTypeConveter (typeof(IodineString), new StringTypeConverter ());
			RegisterTypeConveter (typeof(IodineInteger), new IntegerTypeConverter ());
			RegisterTypeConveter (typeof(IodineBool), new BoolTypeConverter ());
		}

		public void RegisterTypeConveter (Type fromType, ITypeConverter converter)
		{
			conveters [fromType] = converter;
		}

		public bool ConvertToPrimative (IodineObject obj, out object result)
		{
			if (conveters.ContainsKey (obj.GetType ())) {
				return conveters [obj.GetType ()].TryToConvertToPrimative (obj, out result);
			}
			result = null;
			return false;
		}

		public bool ConvertFromPrimative (object obj, out IodineObject result)
		{
			if (conveters.ContainsKey (obj.GetType ())) {
				return conveters [obj.GetType ()].TryToConvertFromPrimative (obj, out result);
			}
			result = null;
			return false;
		}

		public dynamic CreateDynamicObject (IodineEngine engine, IodineObject obj)
		{
			return new IodineDynamicObject (obj, engine.VirtualMachine);
		}
	}
}

