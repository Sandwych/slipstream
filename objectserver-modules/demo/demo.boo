
import ObjectServer.Model
import ObjectServer

[ServiceObject]
public class TestObject(ModelBase):

    def constructor():
        self.Name = "demo.demo_object"
        self.DefineField("name", "姓名", "varchar", 64, true)
        self.DefineField("address", "地址", "varchar", 200, true)

