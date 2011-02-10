
import ObjectServer.Model
import ObjectServer

[ServiceObject]
class TestObject(ModelBase):

    def constructor():
        self.Name = "Demo.DemoObject";
        self.DefineField("name", "姓名", "varchar", 64);
        self.DefineField("address", "地址", "varchar", 200);

