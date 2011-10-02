using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IField : IFieldDescriptor
    {
        FieldValueGetter Getter { get; set; }
        FieldDefaultValueGetter DefaultProc { get; set; }

        void VerifyDefinition();

        Dictionary<long, object> GetFieldValues(
            ITransactionContext scope, ICollection<Dictionary<string, object>> records);

        object SetFieldValue(ITransactionContext scope, object value);

        object BrowseField(ITransactionContext scope, IDictionary<string, object> record);

        #region Fluent interface

        IField SetLabel(string label);
        IField Required();
        IField NotRequired();
        IField SetValueGetter(FieldValueGetter fieldGetter);
        IField SetDefaultValueGetter(FieldDefaultValueGetter defaultProc);
        IField SetSize(int size);
        IField SetHelp(string help);
        IField Readonly();
        IField NotReadonly();
        IField OnDelete(OnDeleteAction act);
        IField BeProperty();
        IField SetOptions(IDictionary<string, string> options);
        IField Unique();
        IField NotUnique();

        #endregion

    }
}
