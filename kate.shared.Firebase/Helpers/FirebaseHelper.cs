using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kate.shared.Helpers
{
    public interface bFirebaseSerializable
    {
        void FromFirebase(DocumentSnapshot document);
        void ToFirebase(DocumentReference document);
        DocumentReference GetFirebaseDocumentReference(FirestoreDb database);
    }
    public static class FirebaseHelper
    {

        public static Dictionary<Type, string> FirebaseCollection = new Dictionary<Type, string>()
        {
        };
        public static string ParseString(DocumentSnapshot document, string key, string defaultValue="") => Parse<string>(document, key, defaultValue ?? "");

        public static T Parse<T>(DocumentSnapshot document, string key, T defaultValue)
        {
            try
            {
                return document.GetValue<T>(key);
            }
            catch
            { }
            return defaultValue;
        }

        public static T ParseDocumentReference<T>(DocumentSnapshot document, string key, T defaultValue) where T : bFirebaseSerializable, new()
        {
            try
            {
                var attemptedValue = Parse<object>(document, key, null);
                if (attemptedValue == null)
                    return defaultValue;
                var f = (DocumentReference)attemptedValue;
                return DeserializeDocumentReference<T>(f);
            }
            catch
            { }
            return new T();
        }

        public static T DeserializeDocumentReference<T>(DocumentReference document) where T : bFirebaseSerializable, new()
        {
            var filePath = document.Path.Split("documents/")[1];
            var snapshot = document.Database.Document(filePath).GetSnapshotAsync();
            snapshot.Wait();
            return DeserializeDocumentSnapshot<T>(snapshot.Result);
        }
        public static T DeserializeDocumentSnapshot<T>(DocumentSnapshot snapshot) where T : bFirebaseSerializable, new()
        {
            try
            {
                var obj = new T();
                obj.FromFirebase(snapshot);
                return obj;
            }
            catch
            { }
            return new T();
        }
    }
}
