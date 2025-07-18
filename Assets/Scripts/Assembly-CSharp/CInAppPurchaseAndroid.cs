using System;
using System.Collections.Generic;
using UnityEngine;

public class CInAppPurchaseAndroid : ICInAppPurchase
{
	private IntPtr m_gluGoogleIAP;

	private IntPtr m_buyProductMethod;

	private IntPtr m_isIAPSupportedMethod;

	private IntPtr m_getTransactionStatusMethod;

	private IntPtr m_retrieveProductMethod;

	private IntPtr m_onIAPDestroyMethod;

	private CInAppPurchaseProduct[] m_validProducts;

	public CInAppPurchaseAndroid()
	{
		m_gluGoogleIAP = IntPtr.Zero;
		m_buyProductMethod = IntPtr.Zero;
		m_isIAPSupportedMethod = IntPtr.Zero;
		m_onIAPDestroyMethod = IntPtr.Zero;
	}

	~CInAppPurchaseAndroid()
	{
		if (m_gluGoogleIAP != IntPtr.Zero)
		{
			//AndroidJNI.CallVoidMethod(m_gluGoogleIAP, m_onIAPDestroyMethod, new jvalue[0]);
			//AndroidJNI.DeleteGlobalRef(m_gluGoogleIAP);
		}
	}

	public override void Init(Dictionary<string, string> products, string marketPublicKey)
	{
		string[] array = new string[products.Keys.Count];
		products.Keys.CopyTo(array, 0);
		Init(array, marketPublicKey);
	}

	public override void Init(string[] products, string marketPublicKey)
	{
		/*if (products != null && marketPublicKey != null)
		{
			IntPtr intPtr = AndroidJNI.FindClass("com/glu/android/iap/GluDebug");
			IntPtr staticFieldID = AndroidJNI.GetStaticFieldID(intPtr, "m_debugLogEnabled", "Z");
			AndroidJNI.SetStaticBooleanField(intPtr, staticFieldID, false);
			IntPtr intPtr2 = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
			IntPtr staticFieldID2 = AndroidJNI.GetStaticFieldID(intPtr2, "currentActivity", "Landroid/app/Activity;");
			IntPtr staticObjectField = AndroidJNI.GetStaticObjectField(intPtr2, staticFieldID2);
			IntPtr intPtr3 = AndroidJNI.FindClass("com/glu/android/iap/GluGoogleIAP");
			IntPtr methodID = AndroidJNI.GetMethodID(intPtr3, "<init>", "(Landroid/app/Activity;Ljava/lang/String;)V");
			IntPtr intPtr4 = AndroidJNI.NewStringUTF(marketPublicKey);
			jvalue[] array = new jvalue[2];
			array[0].l = staticObjectField;
			array[1].l = intPtr4;
			IntPtr obj = AndroidJNI.NewObject(intPtr3, methodID, array);
			m_gluGoogleIAP = AndroidJNI.NewGlobalRef(obj);
			m_buyProductMethod = AndroidJNI.GetMethodID(intPtr3, "buyProduct", "(Ljava/lang/String;)V");
			m_onIAPDestroyMethod = AndroidJNI.GetMethodID(intPtr3, "onIAPDestroy", "()V");
			m_isIAPSupportedMethod = AndroidJNI.GetMethodID(intPtr3, "isIAPSupported", "()Z");
			m_getTransactionStatusMethod = AndroidJNI.GetMethodID(intPtr3, "getTransactionStatus", "()I");
			m_retrieveProductMethod = AndroidJNI.GetMethodID(intPtr3, "retrieveProduct", "()Ljava/lang/String;");
			AndroidJNI.DeleteLocalRef(intPtr4);
			AndroidJNI.DeleteLocalRef(intPtr3);
			AndroidJNI.DeleteLocalRef(staticObjectField);
			AndroidJNI.DeleteLocalRef(intPtr2);
			AndroidJNI.DeleteLocalRef(intPtr);
			m_validProducts = new CInAppPurchaseProduct[products.GetLength(0)];
			for (int i = 0; i < products.GetLength(0); i++)
			{
				m_validProducts[i] = new CInAppPurchaseProduct(products[i]);
			}
		}*/
	}

	public override CInAppPurchaseProduct[] GetAvailableProducts()
	{
		return m_validProducts;
	}

	public override void BuyProduct(string product)
	{
		/*if (m_validProducts != null)
		{
			IntPtr intPtr = AndroidJNI.NewStringUTF(product);
			jvalue[] array = new jvalue[1];
			array[0].l = intPtr;
			AndroidJNI.CallVoidMethod(m_gluGoogleIAP, m_buyProductMethod, array);
			AndroidJNI.DeleteLocalRef(intPtr);
		}*/
	}

	public override bool IsTurnedOn()
	{
		if (m_validProducts == null)
		{
		}
		return true;
	}

	public override bool IsAvailable()
	{
		/*if (m_validProducts != null)
		{
			return AndroidJNI.CallBooleanMethod(m_gluGoogleIAP, m_isIAPSupportedMethod, new jvalue[0]);
		}*/
		return false;
	}

	public override string RetrieveProduct()
	{
		/*if (m_validProducts != null)
		{
			return AndroidJNI.CallStringMethod(m_gluGoogleIAP, m_retrieveProductMethod, new jvalue[0]);
		}*/
		return null;
	}

	public override TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		/*if (m_validProducts != null)
		{
			return (TRANSACTION_STATE)AndroidJNI.CallIntMethod(m_gluGoogleIAP, m_getTransactionStatusMethod, new jvalue[0]);
		}*/
		return TRANSACTION_STATE.NONE;
	}

	public override void RestoreCompletedTransactions()
	{
	}

	public override RESTORE_STATE GetRestoreStatus()
	{
		return RESTORE_STATE.SUCCESS_EMPTY;
	}

	public override int GetRetrievalQueueCount()
	{
		return 0;
	}

	public override string GetRetrievalQueueItem(int index)
	{
		return null;
	}

	public override void RetrievalQueueDispose(int numItems)
	{
	}

	private string getJNIObjectClassName(IntPtr obj)
	{
		/*IntPtr intPtr = AndroidJNI.FindClass("java/lang/Class");
		IntPtr intPtr2 = AndroidJNI.FindClass("java/lang/Object");
		IntPtr methodID = AndroidJNI.GetMethodID(intPtr, "getName", "()Ljava/lang/String;");
		IntPtr methodID2 = AndroidJNI.GetMethodID(intPtr2, "getClass", "()Ljava/lang/Class;");
		IntPtr obj2 = AndroidJNI.CallObjectMethod(obj, methodID2, new jvalue[0]);
		string result = AndroidJNI.CallStringMethod(obj2, methodID, new jvalue[0]);
		AndroidJNI.DeleteLocalRef(obj2);
		AndroidJNI.DeleteLocalRef(intPtr2);
		AndroidJNI.DeleteLocalRef(intPtr);
		return result;*/
		return "";
	}
}
