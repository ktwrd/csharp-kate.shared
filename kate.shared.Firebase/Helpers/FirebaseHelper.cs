using Google.Cloud.Firestore;
using kate.shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kate.shared.Helpers
{
    public interface bFirebaseSerializable
    {
        Task FromFirebase(DocumentSnapshot document, VoidDelegate completeIncrement);
        Task ToFirebase(DocumentReference document, VoidDelegate completeIncrement);
        DocumentReference GetFirebaseDocumentReference(FirestoreDb database);
    }
    public static class FirebaseHelper
    {

        public static Dictionary<Type, string> FirebaseCollection = new Dictionary<Type, string>()
        { };
        public static string ParseString(DocumentSnapshot document, string key, string defaultValue="") => Parse<string>(document, key, defaultValue);

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

        public async static Task<T> ParseDocumentReference<T>(VoidDelegate completeIncrement, DocumentSnapshot document, string key, T defaultValue) where T : bFirebaseSerializable, new()
        {
            try
            {
                var attemptedValue = Parse<object>(document, key, new object());
                if (attemptedValue == null)
                    return defaultValue;
                var f = (DocumentReference)attemptedValue;
                return await DeserializeDocumentReference<T>(f, completeIncrement);
            }
            catch
            { }
            return new T();
        }

        public async static Task<T> DeserializeDocumentReference<T>(DocumentReference document, VoidDelegate completeIncrement) where T : bFirebaseSerializable, new()
        {
            var filePath = document.Path.Split("documents/")[1];
            var snapshot = await document.Database.Document(filePath).GetSnapshotAsync();
            return DeserializeDocumentSnapshot<T>(snapshot, completeIncrement);
        }
        public static T DeserializeDocumentSnapshot<T>(DocumentSnapshot snapshot, VoidDelegate completeIncrement) where T : bFirebaseSerializable, new()
        {
            try
            {
                var obj = new T();
                obj.FromFirebase(snapshot, completeIncrement);
                return obj;
            }
            catch
            { }
            return new T();
        }
    }
}
