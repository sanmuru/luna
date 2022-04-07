-- [[定义命名空间]]
-- 第一种：长命名空间字符串
LCore<namespace> = "SamLu.Core" -- 编译器自行将字符串转写为Lua表嵌套形式。
-- 第二种：嵌套命名空间
SamLu<namespace> = "SamLu"
Lua<namespace> = "Lua"; SamLu<namespace>(Lua<namespace>) -- 加入一个游离的命名空间。
LCore<namespace> = SamLu<namespace>("Lua.Core") -- 此处命名空间类型变量SamLu的变量特性可省略。

-- [[定义接口]]
ITuple<interface> = LCore("ITuple")

-- [[定义泛型接口]]
ITuple<interface>`T1`T2`T3`T4`T5`T6`T7`TRest = LCore("ITuple", T1, T2, T3, T4, T5, T6, T7, TRest) -- 编译器自行添加泛型后缀，以与非泛型或其他泛型类型形成区分。