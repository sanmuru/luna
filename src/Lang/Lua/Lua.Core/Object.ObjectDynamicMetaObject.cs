// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;

namespace SamLu.Lua;

partial class Object
{
    protected class ObjectDynamicMetaObject : DynamicMetaObject
    {
        public ObjectDynamicMetaObject(Expression expression, Object value) : base(expression, BindingRestrictions.Empty, value) { }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (indexes.Length > 1) throw new ArgumentException("Multi-index not supported.", nameof(indexes));

            return this.BindGetValueByKey(indexes[0].Expression);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (indexes.Length > 1) throw new ArgumentException("Multi-index not supported.", nameof(indexes));

            return this.BindSetValueByKey(indexes[0].Expression, value.Expression);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return this.BindGetValueByKey(Expression.Constant(binder.Name));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return this.BindSetValueByKey(Expression.Constant(binder.Name), value.Expression);
        }

        private DynamicMetaObject BindGetValueByKey(Expression key)
        {
            var getValueByKey = this.GetIndexExpression(key);
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new(getValueByKey, restrictions);
        }

        private DynamicMetaObject BindSetValueByKey(Expression key, Expression value)
        {
            Expression setValueByKey = Expression.Assign(this.GetIndexExpression(key), value);
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new(setValueByKey, restrictions);
        }

        private Expression GetIndexExpression(Expression key)
        {
            Expression getValueByKey = Expression.MakeIndex(
                instance: Expression.Convert(this.Expression, this.LimitType), // 获取this引用。
                indexer: this.LimitType.GetProperty("Item"), // 反射获取索引器。
                arguments: new[] // 将参数key转型为Object。
                {
                    Expression.ConvertChecked(key, typeof(Object))
                }
            );
            return getValueByKey;
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            Expression[] exprs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) exprs[i] = args[i].Expression;
            Expression invokeExpression = this.GetInvokeExpression(Expression.Convert(this.Expression, this.LimitType), exprs);
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new(invokeExpression, restrictions);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            Expression[] exprs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) exprs[i] = args[i].Expression;
            Expression invokeExpression = this.GetInvokeExpression(this.GetIndexExpression(Expression.Constant(binder.Name)), exprs);
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            return new(invokeExpression, restrictions);
        }

        private Expression GetInvokeExpression(Expression instance, Expression[] args)
        {
            return Expression.Call(instance, this.Value.GetType().GetMethod("Invoke"), args);
        }
    }
}
